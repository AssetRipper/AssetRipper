using System.Text.Json;

namespace AssetRipper.Export.UnityProjects.Scripts.UnityPackages;

/// <summary>
/// Finds an installed Unity editor whose bundled packages can be referenced by an exported project.
/// </summary>
public static class UnityEditorLocator
{
	/// <summary>
	/// Find the editor to use for a project.
	/// </summary>
	/// <param name="projectVersion">The Unity version of the exported project.</param>
	/// <param name="configuredPath">An editor directory chosen by the user, or null to search Unity Hub's install locations.</param>
	/// <remarks>
	/// An exact match is preferred. Otherwise the newest installed editor with the same major version that is at least as new as the project is used,
	/// since Unity upgrades projects to newer versions but not to older ones, and packages change substantially between major versions.
	/// </remarks>
	public static bool TryFind(UnityVersion projectVersion, string? configuredPath, [NotNullWhen(true)] out string? editorDirectory, out UnityVersion editorVersion)
	{
		if (!string.IsNullOrEmpty(configuredPath))
		{
			if (UnityPackageIndex.GetPackageManagerDirectory(configuredPath) is not null)
			{
				editorDirectory = configuredPath;
				editorVersion = TryParseVersion(Path.GetFileName(Path.TrimEndingDirectorySeparator(configuredPath)), out UnityVersion version) ? version : projectVersion;
				return true;
			}
			editorDirectory = null;
			editorVersion = default;
			return false;
		}

		List<(string Directory, UnityVersion Version)> installs = [];
		foreach (string hubDirectory in GetHubEditorDirectories())
		{
			if (!Directory.Exists(hubDirectory))
			{
				continue;
			}
			foreach (string directory in Directory.EnumerateDirectories(hubDirectory))
			{
				if (TryParseVersion(Path.GetFileName(directory), out UnityVersion version) && UnityPackageIndex.GetPackageManagerDirectory(directory) is not null)
				{
					installs.Add((directory, version));
				}
			}
		}

		foreach ((string directory, UnityVersion version) in installs
			.Where(i => i.Version == projectVersion)
			.Concat(installs.Where(i => i.Version.Major == projectVersion.Major && i.Version >= projectVersion).OrderByDescending(i => i.Version)))
		{
			editorDirectory = directory;
			editorVersion = version;
			return true;
		}
		editorDirectory = null;
		editorVersion = default;
		return false;
	}

	private static bool TryParseVersion(string text, out UnityVersion version)
	{
		try
		{
			version = UnityVersion.Parse(text);
			return version.Major > 0;
		}
		catch (Exception)
		{
			version = default;
			return false;
		}
	}

	private static IEnumerable<string> GetHubEditorDirectories()
	{
		if (OperatingSystem.IsWindows())
		{
			yield return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Unity", "Hub", "Editor");
		}
		else if (OperatingSystem.IsMacOS())
		{
			yield return "/Applications/Unity/Hub/Editor";
		}
		else
		{
			yield return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Unity", "Hub", "Editor");
		}

		// A custom install location chosen in Unity Hub
		string secondaryInstallPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UnityHub", "secondaryInstallPath.json");
		if (File.Exists(secondaryInstallPath))
		{
			string? path = null;
			try
			{
				using JsonDocument document = JsonDocument.Parse(File.ReadAllText(secondaryInstallPath));
				path = document.RootElement.ValueKind == JsonValueKind.String ? document.RootElement.GetString() : null;
			}
			catch (JsonException)
			{
			}
			if (!string.IsNullOrEmpty(path))
			{
				yield return path;
			}
		}
	}
}
