using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects.Scripts.UnityPackages;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Scripts;

public class ScriptExporter : IAssetExporter
{
	public ScriptExporter(IAssemblyManager assemblyManager, FullConfiguration configuration)
	{
		AssemblyManager = assemblyManager;
		Decompiler = new ScriptDecompiler(AssemblyManager)
		{
			LanguageVersion = configuration.ExportSettings.ScriptLanguageVersion.ToCSharpLanguageVersion(configuration.Version),
			ScriptContentLevel = configuration.ImportSettings.ScriptContentLevel,
			FullyQualifiedTypeNames = configuration.ExportSettings.ScriptTypesFullyQualified,
		};
		ExportMode = configuration.ExportSettings.ScriptExportMode;
		ReferenceAssemblyDictionary = ReferenceAssemblies.GetReferenceAssemblies(AssemblyManager, configuration.Version);
		if (configuration.ExportSettings.ReferenceUnityPackages && AssemblyManager.IsSet)
		{
			PackageIndex = UnityPackageIndex.GetForProject(configuration.Version, configuration.ExportSettings.UnityEditorPath);
			if (PackageIndex is null)
			{
				Logger.Warning(LogCategory.Export, $"No Unity editor installation was found for {configuration.Version}, so package assemblies will be exported as plugins.");
			}
			else
			{
				foreach (AsmResolver.DotNet.AssemblyDefinition assembly in AssemblyManager.GetAssemblies())
				{
					IReadOnlyList<PackageAssembly> replacements = PackageIndex.GetReplacements(assembly);
					if (replacements.Count > 0)
					{
						packageReplacements[assembly.Name!] = replacements;
					}
				}
			}
		}
	}

	public IAssemblyManager AssemblyManager { get; }
	public ScriptExportMode ExportMode { get; }
	/// <summary>
	/// The packages that assemblies can be replaced with, or null if packages aren't referenced.
	/// </summary>
	public UnityPackageIndex? PackageIndex { get; }
	/// <summary>
	/// The package assemblies replacing each game assembly that comes from a package.
	/// </summary>
	private readonly Dictionary<string, IReadOnlyList<PackageAssembly>> packageReplacements = new();
	private readonly HashSet<string> missingPackageScripts = new();
	internal ScriptDecompiler Decompiler { get; }
	internal Dictionary<string, UnityGuid> ReferenceAssemblyDictionary { get; }
	private bool HasDecompiled { get; set; } = false;
	private static long MonoScriptDecompiledFileID { get; } = ExportIdHandler.GetMainExportID((int)ClassIDType.MonoScript);

	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset is IMonoScript script)
		{
			if (HasDecompiled)
			{
				exportCollection = new SingleRedirectExportCollection(asset, CreateExportPointer(script));
			}
			else
			{
				HasDecompiled = true;
				if (AssemblyManager.IsSet)
				{
					exportCollection = new ScriptExportCollection(this, script);
				}
				else
				{
					exportCollection = new EmptyScriptExportCollection(this, script);
				}
			}
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}

	public AssemblyExportType GetExportType(IMonoScript script)
	{
		return GetExportType(script.GetAssemblyNameFixed());
	}

	public MetaPtr CreateExportPointer(IMonoScript script)
	{
		return GetExportType(script) switch
		{
			AssemblyExportType.Decompile => new(MonoScriptDecompiledFileID, ScriptHashing.CalculateScriptGuid(script), AssetType.Meta),
			AssemblyExportType.Skip => new(ScriptHashing.CalculateScriptFileID(script), ReferenceAssemblyDictionary[script.GetAssemblyNameFixed()], AssetType.Meta),
			AssemblyExportType.Package => CreatePackageExportPointer(script),
			_ => new(ScriptHashing.CalculateScriptFileID(script), ScriptHashing.CalculateAssemblyGuid(script), AssetType.Meta),
		};
	}

	private MetaPtr CreatePackageExportPointer(IMonoScript script)
	{
		string assemblyName = script.GetAssemblyNameFixed();
		IReadOnlyList<PackageAssembly> replacements = packageReplacements[assemblyName];
		string @namespace = script.Namespace.String;
		string className = script.GetNonGenericClassName();
		foreach (PackageAssembly assembly in replacements)
		{
			if (assembly.TryGetScriptGuid(@namespace, className, out UnityGuid guid))
			{
				// Like decompiled scripts, a script file in a package has the main object of the MonoScript class.
				return new(MonoScriptDecompiledFileID, guid, AssetType.Meta);
			}
		}

		lock (missingPackageScripts)
		{
			if (missingPackageScripts.Add($"{assemblyName}:{@namespace}.{className}"))
			{
				// Builds contain a MonoScript for many types that are never used as components, so this is usually harmless.
				string packages = string.Join(", ", replacements.Select(a => $"{a.PackageName} {a.PackageVersion}").Distinct());
				Logger.Info(LogCategory.Export, $"Script {@namespace}.{className} was not found in {packages}. Any component using it will have a missing script.");
			}
		}
		return new(ScriptHashing.CalculateScriptFileID(script), ScriptHashing.CalculateAssemblyGuid(script), AssetType.Meta);
	}

	public AssemblyExportType GetExportType(string assemblyName)
	{
		if (ReferenceAssemblyDictionary.ContainsKey(assemblyName))
		{
			return AssemblyExportType.Skip;
		}
		else if (packageReplacements.ContainsKey(assemblyName))
		{
			return AssemblyExportType.Package;
		}
		else if (!AssemblyManager.IsSet)
		{
			return AssemblyExportType.Decompile;
		}
		else if (ExportMode is ScriptExportMode.Decompiled)
		{
			return AssemblyExportType.Decompile;
		}
		else if (ExportMode is ScriptExportMode.Hybrid)
		{
			return ReferenceAssemblies.IsPredefinedAssembly(assemblyName)
				? AssemblyExportType.Decompile
				: AssemblyExportType.Save;
		}
		else
		{
			return AssemblyExportType.Save;
		}
	}

	AssetType IAssetExporter.ToExportType(IUnityObjectBase asset) => AssetType.Meta;

	bool IAssetExporter.ToUnknownExportType(Type type, out AssetType assetType)
	{
		assetType = AssetType.Meta;
		return true;
	}
}
