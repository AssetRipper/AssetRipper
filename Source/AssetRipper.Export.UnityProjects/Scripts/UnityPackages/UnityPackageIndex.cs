using AssetRipper.Import.Logging;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AssetRipper.Export.UnityProjects.Scripts.UnityPackages;

/// <summary>
/// An index of the Unity packages bundled with a Unity editor installation.
/// </summary>
/// <remarks>
/// Each assembly defined by a package is mapped to the package and to the guids of its script files,
/// so that game assemblies built from a package can be replaced with a reference to that package.
/// </remarks>
public sealed partial class UnityPackageIndex
{
	private readonly Dictionary<string, PackageAssembly> assemblies;

	/// <summary>
	/// The directory of the editor installation that the packages were read from.
	/// </summary>
	public string EditorDirectory { get; }

	public UnityVersion EditorVersion { get; }

	private UnityPackageIndex(string editorDirectory, UnityVersion editorVersion, Dictionary<string, PackageAssembly> assemblies)
	{
		EditorDirectory = editorDirectory;
		EditorVersion = editorVersion;
		this.assemblies = assemblies;
	}

	public bool TryGetAssembly(string assemblyName, [NotNullWhen(true)] out PackageAssembly? assembly)
	{
		return assemblies.TryGetValue(assemblyName, out assembly);
	}

	private static readonly Dictionary<string, UnityPackageIndex> cache = new();

	/// <summary>
	/// Get the package index of the editor to use for a project, or null if no editor was found.
	/// </summary>
	/// <remarks>
	/// Reading the packages takes a while, so the index is cached for each editor.
	/// </remarks>
	public static UnityPackageIndex? GetForProject(UnityVersion projectVersion, string? configuredEditorPath)
	{
		if (!UnityEditorLocator.TryFind(projectVersion, configuredEditorPath, out string? editorDirectory, out UnityVersion editorVersion))
		{
			return null;
		}
		lock (cache)
		{
			if (!cache.TryGetValue(editorDirectory, out UnityPackageIndex? index))
			{
				Logger.Info(LogCategory.Export, $"Reading the packages bundled with Unity {editorVersion} at '{editorDirectory}'...");
				index = Load(editorDirectory, editorVersion);
				Logger.Info(LogCategory.Export, $"Found {index.assemblies.Count} package assemblies");
				cache.Add(editorDirectory, index);
			}
			return index;
		}
	}

	public static UnityPackageIndex Load(string editorDirectory, UnityVersion editorVersion)
	{
		string packageManagerDirectory = GetPackageManagerDirectory(editorDirectory)
			?? throw new DirectoryNotFoundException($"No package manager directory was found in '{editorDirectory}'.");

		Dictionary<string, PackageAssembly> assemblies = new();
		string builtInDirectory = Path.Join(packageManagerDirectory, "BuiltInPackages");
		if (Directory.Exists(builtInDirectory))
		{
			foreach (string packageDirectory in Directory.EnumerateDirectories(builtInDirectory))
			{
				AddPackage(assemblies, ReadDirectory(packageDirectory));
			}
		}
		string archiveDirectory = Path.Join(packageManagerDirectory, "Editor");
		if (Directory.Exists(archiveDirectory))
		{
			foreach (string archivePath in Directory.EnumerateFiles(archiveDirectory, "*.tgz"))
			{
				try
				{
					AddPackage(assemblies, ReadArchive(archivePath));
				}
				catch (Exception ex) when (ex is IOException or InvalidDataException or JsonException)
				{
					Logger.Warning(LogCategory.Export, $"Could not read package archive '{archivePath}': {ex.Message}");
				}
			}
		}
		return new UnityPackageIndex(editorDirectory, editorVersion, assemblies);
	}

	/// <summary>
	/// Find the directory containing the bundled packages, given the root directory of an editor installation.
	/// </summary>
	public static string? GetPackageManagerDirectory(string editorDirectory)
	{
		string[] candidates =
		[
			editorDirectory,
			Path.Join(editorDirectory, "Editor", "Data", "Resources", "PackageManager"), // Windows and Linux
			Path.Join(editorDirectory, "Data", "Resources", "PackageManager"),
			Path.Join(editorDirectory, "Unity.app", "Contents", "Resources", "PackageManager"), // macOS
		];
		return candidates.FirstOrDefault(c => Directory.Exists(Path.Join(c, "BuiltInPackages")) || Directory.Exists(Path.Join(c, "Editor")) && Path.GetFileName(c) == "PackageManager");
	}

	private static void AddPackage(Dictionary<string, PackageAssembly> assemblies, PackageContents? contents)
	{
		if (contents is null)
		{
			return;
		}
		foreach (PackageAssembly assembly in contents.CreateAssemblies())
		{
			// Assembly names are unique within an editor, so a duplicate means a malformed package.
			assemblies.TryAdd(assembly.AssemblyName, assembly);
		}
	}

	private static PackageContents? ReadDirectory(string packageDirectory)
	{
		PackageContents contents = new();
		foreach (string path in Directory.EnumerateFiles(packageDirectory, "*", SearchOption.AllDirectories))
		{
			if (PackageContents.IsRelevant(path))
			{
				string relativePath = Path.GetRelativePath(packageDirectory, path).Replace('\\', '/');
				contents.Add(relativePath, File.ReadAllText(path));
			}
		}
		return contents.IsValid ? contents : null;
	}

	private static PackageContents? ReadArchive(string archivePath)
	{
		PackageContents contents = new();
		using FileStream fileStream = File.OpenRead(archivePath);
		using GZipStream gzipStream = new(fileStream, CompressionMode.Decompress);
		using TarReader reader = new(gzipStream);
		while (reader.GetNextEntry() is { } entry)
		{
			if (entry.EntryType is not (TarEntryType.RegularFile or TarEntryType.V7RegularFile) || entry.DataStream is null)
			{
				continue;
			}
			if (PackageContents.IsRelevant(entry.Name))
			{
				// Archive entries are rooted in a "package" directory.
				string relativePath = entry.Name.StartsWith("package/", StringComparison.Ordinal) ? entry.Name["package/".Length..] : entry.Name;
				using StreamReader streamReader = new(entry.DataStream);
				contents.Add(relativePath, streamReader.ReadToEnd());
			}
		}
		return contents.IsValid ? contents : null;
	}

	/// <summary>
	/// The files of a package that are needed to map its scripts.
	/// </summary>
	private sealed class PackageContents
	{
		private string? packageName;
		private string? packageVersion;
		private readonly Dictionary<string, string> assemblyDefinitions = new(); // directory => assembly name
		private readonly Dictionary<string, string> assemblyReferences = new(); // directory => referenced assembly name or guid
		private readonly Dictionary<string, string> assemblyDefinitionGuids = new(); // guid => assembly name
		private readonly Dictionary<string, string> scripts = new(); // path => source
		private readonly Dictionary<string, UnityGuid> metaGuids = new(); // asset path => guid
		private readonly Dictionary<string, string> assemblyDefinitionPaths = new(); // path => assembly name

		public bool IsValid => packageName is not null && packageVersion is not null && assemblyDefinitions.Count > 0;

		public static bool IsRelevant(string path)
		{
			// Unity ignores folders ending with a tilde, such as "Samples~" and "Tests~".
			if (path.Replace('\\', '/').Split('/').Any(segment => segment.EndsWith('~')))
			{
				return false;
			}
			return path.EndsWith(".cs", StringComparison.Ordinal)
				|| path.EndsWith(".cs.meta", StringComparison.Ordinal)
				|| path.EndsWith(".asmdef", StringComparison.Ordinal)
				|| path.EndsWith(".asmdef.meta", StringComparison.Ordinal)
				|| path.EndsWith(".asmref", StringComparison.Ordinal)
				|| path.EndsWith("package.json", StringComparison.Ordinal);
		}

		public void Add(string relativePath, string text)
		{
			string directory = GetDirectory(relativePath);
			if (relativePath == "package.json")
			{
				using JsonDocument document = JsonDocument.Parse(text, JsonOptions);
				packageName = document.RootElement.GetProperty("name").GetString();
				packageVersion = document.RootElement.GetProperty("version").GetString();
			}
			else if (relativePath.EndsWith(".asmdef", StringComparison.Ordinal))
			{
				using JsonDocument document = JsonDocument.Parse(text, JsonOptions);
				string? name = document.RootElement.GetProperty("name").GetString();
				if (!string.IsNullOrEmpty(name))
				{
					assemblyDefinitions[directory] = name;
					assemblyDefinitionPaths[relativePath] = name;
				}
			}
			else if (relativePath.EndsWith(".asmref", StringComparison.Ordinal))
			{
				using JsonDocument document = JsonDocument.Parse(text, JsonOptions);
				string? reference = document.RootElement.GetProperty("reference").GetString();
				if (!string.IsNullOrEmpty(reference))
				{
					assemblyReferences[directory] = reference;
				}
			}
			else if (relativePath.EndsWith(".meta", StringComparison.Ordinal))
			{
				if (TryParseMetaGuid(text, out UnityGuid guid))
				{
					metaGuids[relativePath[..^".meta".Length]] = guid;
				}
			}
			else
			{
				scripts[relativePath] = text;
			}
		}

		public IEnumerable<PackageAssembly> CreateAssemblies()
		{
			foreach ((string path, string name) in assemblyDefinitionPaths)
			{
				if (metaGuids.TryGetValue(path, out UnityGuid guid))
				{
					assemblyDefinitionGuids[guid.ToString()] = name;
				}
			}

			Dictionary<string, PackageAssembly> result = new();
			foreach (string name in assemblyDefinitions.Values)
			{
				result.TryAdd(name, new PackageAssembly(name, packageName!, packageVersion!));
			}

			foreach ((string path, string source) in scripts)
			{
				if (!metaGuids.TryGetValue(path, out UnityGuid guid)
					|| GetOwningAssembly(GetDirectory(path)) is not { } assemblyName
					|| !result.TryGetValue(assemblyName, out PackageAssembly? assembly))
				{
					continue;
				}

				// Unity associates a script file with the type that has the same name as the file.
				string className = Path.GetFileNameWithoutExtension(path);
				if (TryFindNamespace(source, className, out string? @namespace))
				{
					assembly.AddScript(@namespace, className, guid);
				}
			}

			return result.Values;
		}

		private string? GetOwningAssembly(string directory)
		{
			while (true)
			{
				if (assemblyDefinitions.TryGetValue(directory, out string? name))
				{
					return name;
				}
				if (assemblyReferences.TryGetValue(directory, out string? reference))
				{
					const string GuidPrefix = "GUID:";
					return reference.StartsWith(GuidPrefix, StringComparison.Ordinal)
						? assemblyDefinitionGuids.GetValueOrDefault(reference[GuidPrefix.Length..])
						: reference;
				}
				if (directory.Length == 0)
				{
					return null;
				}
				directory = GetDirectory(directory);
			}
		}

		private static string GetDirectory(string path)
		{
			int index = path.LastIndexOf('/');
			return index < 0 ? "" : path[..index];
		}
	}

	private static readonly JsonDocumentOptions JsonOptions = new()
	{
		AllowTrailingCommas = true,
		CommentHandling = JsonCommentHandling.Skip,
	};

	private static bool TryParseMetaGuid(string text, out UnityGuid guid)
	{
		Match match = MetaGuidRegex().Match(text);
		if (match.Success)
		{
			guid = UnityGuid.Parse(match.Groups[1].Value);
			return true;
		}
		guid = default;
		return false;
	}

	/// <summary>
	/// Find the namespace of the type declared with <paramref name="className"/>.
	/// </summary>
	/// <remarks>
	/// This is a lightweight scan rather than a full parse. It takes the last namespace declared before the type,
	/// which is correct for the one namespace per file that Unity packages use.
	/// </remarks>
	private static bool TryFindNamespace(string source, string className, [NotNullWhen(true)] out string? @namespace)
	{
		Match typeMatch = Regex.Match(source, $@"\b(?:class|struct|interface|enum|record)\s+{Regex.Escape(className)}\b");
		if (!typeMatch.Success)
		{
			@namespace = null;
			return false;
		}
		@namespace = "";
		foreach (Match match in NamespaceRegex().Matches(source[..typeMatch.Index]))
		{
			@namespace = match.Groups[1].Value;
		}
		return true;
	}

	[GeneratedRegex(@"^guid:\s*([0-9a-fA-F]{32})", RegexOptions.Multiline)]
	private static partial Regex MetaGuidRegex();

	[GeneratedRegex(@"^\s*namespace\s+([\w.]+)", RegexOptions.Multiline)]
	private static partial Regex NamespaceRegex();
}
