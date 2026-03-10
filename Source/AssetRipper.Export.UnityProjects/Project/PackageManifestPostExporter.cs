using AsmResolver.DotNet;
using AssetRipper.Export.Configuration;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AssetRipper.Export.UnityProjects.Project;

public partial class PackageManifestPostExporter : IPostExporter
{
	private readonly RegistryPackageBridge? registryPackageBridge;

	public PackageManifestPostExporter()
	{
	}

	public PackageManifestPostExporter(RegistryPackageBridge registryPackageBridge)
	{
		this.registryPackageBridge = registryPackageBridge;
	}

	public void DoPostExport(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		string packagesDirectory = fileSystem.Path.Join(settings.ProjectRootPath, "Packages");
		fileSystem.Directory.Create(packagesDirectory);

		PackageManifest manifest = CreateManifest(settings.Version);
		Dictionary<string, DetectedPackage> discoveredPackages = ScanDependencies(gameData, settings, fileSystem);
		InjectDiscoveredDependencies(manifest, discoveredPackages);

		string path = fileSystem.Path.Join(packagesDirectory, "manifest.json");
		using Stream stream = fileSystem.File.Create(path);
		manifest.Save(stream);

		if (registryPackageBridge is not null)
		{
			string versionsPath = fileSystem.Path.Join(packagesDirectory, "assetripper_versions.txt");
			fileSystem.File.WriteAllText(versionsPath, registryPackageBridge.BuildVersionsReport());
		}
	}

	protected virtual PackageManifest CreateManifest(UnityVersion version)
	{
		return PackageManifest.CreateDefault(version);
	}

	protected virtual Dictionary<string, DetectedPackage> ScanDependencies(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		Dictionary<string, DetectedPackage> dependencies = new(StringComparer.Ordinal);

		ReadEmbeddedManifestDependencies(gameData, dependencies);
		ReadEmbeddedPackageLockDependencies(gameData, dependencies);
		ScanExportedAssets(settings.AssetsPath, fileSystem, dependencies);
		ScanAssemblyNames(gameData, dependencies);
		ReadRegistryBridgeDependencies(dependencies);

		return dependencies;
	}

	protected virtual void InjectDiscoveredDependencies(PackageManifest manifest, Dictionary<string, DetectedPackage> discoveredPackages)
	{
		foreach ((string packageName, DetectedPackage detection) in discoveredPackages)
		{
			string? explicitVersion = detection.Version;
			if (!string.IsNullOrWhiteSpace(explicitVersion))
			{
				manifest.Dependencies[packageName] = explicitVersion;
			}
		}
	}

	private void ReadRegistryBridgeDependencies(Dictionary<string, DetectedPackage> dependencies)
	{
		if (registryPackageBridge is null)
		{
			return;
		}

		foreach ((string packageName, string packageVersion) in registryPackageBridge.ManifestDependencies)
		{
			AddDependency(dependencies, packageName, packageVersion, PackageVersionSource.DeepScan);
		}
	}

	private static void ReadEmbeddedManifestDependencies(GameData gameData, Dictionary<string, DetectedPackage> dependencies)
	{
		foreach (ITextAsset textAsset in gameData.GameBundle.FetchAssets().OfType<ITextAsset>())
		{
			if (textAsset.OriginalPath is null)
			{
				continue;
			}

			if (!textAsset.OriginalPath.EndsWith("Packages/manifest.json", StringComparison.OrdinalIgnoreCase)
				&& !textAsset.OriginalPath.EndsWith("Packages\\manifest.json", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			string jsonText = textAsset.Script_C49.String;
			if (string.IsNullOrWhiteSpace(jsonText))
			{
				continue;
			}

			try
			{
				using JsonDocument document = JsonDocument.Parse(jsonText);
				if (document.RootElement.TryGetProperty("dependencies", out JsonElement dependenciesElement)
					&& dependenciesElement.ValueKind == JsonValueKind.Object)
				{
					foreach (JsonProperty property in dependenciesElement.EnumerateObject())
					{
						if (property.Value.ValueKind == JsonValueKind.String)
						{
							AddDependency(dependencies, property.Name, property.Value.GetString(), PackageVersionSource.Manifest);
						}
					}
				}
			}
			catch
			{
				// Ignore malformed embedded manifests and continue with heuristics.
			}
		}
	}

	private static void ReadEmbeddedPackageLockDependencies(GameData gameData, Dictionary<string, DetectedPackage> dependencies)
	{
		foreach (ITextAsset textAsset in gameData.GameBundle.FetchAssets().OfType<ITextAsset>())
		{
			if (textAsset.OriginalPath is null)
			{
				continue;
			}

			if (!textAsset.OriginalPath.EndsWith("Packages/packages-lock.json", StringComparison.OrdinalIgnoreCase)
				&& !textAsset.OriginalPath.EndsWith("Packages\\packages-lock.json", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			string jsonText = textAsset.Script_C49.String;
			if (string.IsNullOrWhiteSpace(jsonText))
			{
				continue;
			}

			try
			{
				using JsonDocument document = JsonDocument.Parse(jsonText);
				if (document.RootElement.TryGetProperty("dependencies", out JsonElement dependenciesElement)
					&& dependenciesElement.ValueKind == JsonValueKind.Object)
				{
					foreach (JsonProperty property in dependenciesElement.EnumerateObject())
					{
						if (property.Value.ValueKind == JsonValueKind.Object
							&& property.Value.TryGetProperty("version", out JsonElement versionElement)
							&& versionElement.ValueKind == JsonValueKind.String)
						{
							AddDependency(dependencies, property.Name, versionElement.GetString(), PackageVersionSource.PackageLock);
						}
					}
				}
			}
			catch
			{
				// Ignore malformed lock files and continue with heuristics.
			}
		}
	}

	private static void ScanExportedAssets(string assetsPath, FileSystem fileSystem, Dictionary<string, DetectedPackage> dependencies)
	{
		if (!fileSystem.Directory.Exists(assetsPath))
		{
			return;
		}

		foreach (string extension in ScannableAssetExtensions)
		{
			foreach (string filePath in fileSystem.Directory.EnumerateFiles(assetsPath, extension, SearchOption.AllDirectories))
			{
				string text = TryReadAllText(fileSystem, filePath);
				if (text.Length == 0)
				{
					continue;
				}

				AddPackageHintsFromText(text, dependencies);
				AddExplicitPackagesFromText(text, dependencies);
			}
		}
	}

	private static void ScanAssemblyNames(GameData gameData, Dictionary<string, DetectedPackage> dependencies)
	{
		foreach (var assembly in gameData.AssemblyManager.GetAssemblies())
		{
			string assemblyName = assembly.Name ?? string.Empty;
			if (assemblyName.Length == 0)
			{
				continue;
			}
			string? assemblyVersion = TryGetAssemblyVersion(assembly);

			if (assemblyName.StartsWith("UnityEngine.UI", StringComparison.Ordinal)
				|| assemblyName.StartsWith("Unity.UI", StringComparison.Ordinal))
			{
				AddDependency(dependencies, "com.unity.ugui", assemblyVersion, PackageVersionSource.Assembly);
			}
			if (assemblyName.StartsWith("Unity.TextMeshPro", StringComparison.Ordinal)
				|| assemblyName.StartsWith("UnityEngine.TextCoreTextEngine", StringComparison.Ordinal))
			{
				AddDependency(dependencies, "com.unity.textmeshpro", assemblyVersion, PackageVersionSource.Assembly);
			}
			if (assemblyName.StartsWith("Unity.Entities", StringComparison.Ordinal))
			{
				AddDependency(dependencies, "com.unity.entities", assemblyVersion, PackageVersionSource.Assembly);
			}
			if (assemblyName.StartsWith("Unity.RenderPipelines.Universal", StringComparison.Ordinal)
				|| assemblyName.StartsWith("UnityEngine.Rendering.Universal", StringComparison.Ordinal))
			{
				AddDependency(dependencies, "com.unity.render-pipelines.universal", assemblyVersion, PackageVersionSource.Assembly);
			}
			if (assemblyName.StartsWith("Unity.RenderPipelines.HighDefinition", StringComparison.Ordinal)
				|| assemblyName.StartsWith("UnityEngine.Rendering.HighDefinition", StringComparison.Ordinal))
			{
				AddDependency(dependencies, "com.unity.render-pipelines.high-definition", assemblyVersion, PackageVersionSource.Assembly);
			}
			if (assemblyName.StartsWith("Unity.RenderPipelines.Core", StringComparison.Ordinal))
			{
				AddDependency(dependencies, "com.unity.render-pipelines.core", assemblyVersion, PackageVersionSource.Assembly);
			}
			if (assemblyName.StartsWith("Unity.InputSystem", StringComparison.Ordinal)
				|| assemblyName.StartsWith("UnityEngine.InputSystem", StringComparison.Ordinal))
			{
				AddDependency(dependencies, "com.unity.inputsystem", assemblyVersion, PackageVersionSource.Assembly);
			}
			if (assemblyName.StartsWith("Unity.XR.Management", StringComparison.Ordinal))
			{
				AddDependency(dependencies, "com.unity.xr.management", assemblyVersion, PackageVersionSource.Assembly);
			}
			if (assemblyName.StartsWith("Unity.XR.Oculus", StringComparison.Ordinal))
			{
				AddDependency(dependencies, "com.unity.xr.oculus", assemblyVersion, PackageVersionSource.Assembly);
			}
			if (assemblyName.StartsWith("Unity.Netcode", StringComparison.Ordinal))
			{
				AddDependency(dependencies, "com.unity.netcode.gameobjects", assemblyVersion, PackageVersionSource.Assembly);
			}
		}
	}

	private static void AddPackageHintsFromText(string text, Dictionary<string, DetectedPackage> dependencies)
	{
		foreach ((string token, string package) in PackageTokenHints)
		{
			if (text.Contains(token, StringComparison.Ordinal))
			{
				AddDependency(dependencies, package, null, PackageVersionSource.Hint);
			}
		}
	}

	private static void AddExplicitPackagesFromText(string text, Dictionary<string, DetectedPackage> dependencies)
	{
		foreach (Match match in PackageWithVersionRegex().Matches(text))
		{
			string packageName = match.Groups[1].Value;
			string version = match.Groups[2].Value;
			if (LooksLikePackageId(packageName) && !string.IsNullOrWhiteSpace(version))
			{
				AddDependency(dependencies, packageName, version, PackageVersionSource.ExplicitText);
			}
		}

		foreach (Match match in PackageNameRegex().Matches(text))
		{
			string packageName = match.Value;
			if (LooksLikePackageId(packageName))
			{
				AddDependency(dependencies, packageName, null, PackageVersionSource.Hint);
			}
		}
	}

	private static void AddDependency(Dictionary<string, DetectedPackage> dependencies, string packageName, string? version, PackageVersionSource source)
	{
		string normalizedPackageName = NormalizePackageName(packageName);
		if (!LooksLikePackageId(normalizedPackageName))
		{
			return;
		}

		string? normalizedVersion = NormalizeVersion(version);
		DetectedPackage candidate = new(normalizedVersion, source);

		if (!dependencies.TryGetValue(normalizedPackageName, out DetectedPackage existing))
		{
			dependencies.Add(normalizedPackageName, candidate);
			return;
		}

		dependencies[normalizedPackageName] = ChooseBetter(existing, candidate);
	}

	private static DetectedPackage ChooseBetter(DetectedPackage existing, DetectedPackage candidate)
	{
		bool existingHasVersion = !string.IsNullOrWhiteSpace(existing.Version);
		bool candidateHasVersion = !string.IsNullOrWhiteSpace(candidate.Version);

		if (candidateHasVersion && !existingHasVersion)
		{
			return candidate;
		}
		if (!candidateHasVersion && existingHasVersion)
		{
			return existing;
		}
		if (candidate.Source >= existing.Source)
		{
			return candidate;
		}

		return existing;
	}

	private static string NormalizePackageName(string packageName)
	{
		return packageName.Trim().ToLowerInvariant();
	}

	private static bool LooksLikePackageId(string packageName)
	{
		return PackageIdRegex().IsMatch(packageName);
	}

	private static string? NormalizeVersion(string? version)
	{
		if (string.IsNullOrWhiteSpace(version))
		{
			return null;
		}

		string normalizedVersion = version.Trim();
		return normalizedVersion.Length == 0 ? null : normalizedVersion;
	}

	private static string? TryGetAssemblyVersion(AssemblyDefinition assembly)
	{
		Version? version = assembly.Version;
		if (version is null || version.Major < 0 || version.Minor < 0)
		{
			return null;
		}

		int patchVersion = version.Build >= 0 ? version.Build : 0;
		if (version.Major == 0 && version.Minor == 0 && patchVersion == 0)
		{
			return null;
		}

		return $"{version.Major}.{version.Minor}.{patchVersion}";
	}

	private static string TryReadAllText(FileSystem fileSystem, string path)
	{
		try
		{
			return fileSystem.File.ReadAllText(path);
		}
		catch
		{
			return string.Empty;
		}
	}

	private static readonly string[] ScannableAssetExtensions =
	[
		"*.cs",
		"*.shader",
		"*.json",
		"*.asmdef",
		"*.asset",
		"*.prefab",
		"*.unity",
		"*.yaml",
		"*.yml",
	];

	private static readonly (string token, string package)[] PackageTokenHints =
	[
		("namespace UnityEngine.UI", "com.unity.ugui"),
		("using UnityEngine.UI", "com.unity.ugui"),
		("using TMPro", "com.unity.textmeshpro"),
		("namespace TMPro", "com.unity.textmeshpro"),
		("using Unity.Entities", "com.unity.entities"),
		("namespace Unity.Entities", "com.unity.entities"),
		("UnityEngine.Rendering.Universal", "com.unity.render-pipelines.universal"),
		("Universal Render Pipeline", "com.unity.render-pipelines.universal"),
		("RenderPipeline\"=\"UniversalPipeline", "com.unity.render-pipelines.universal"),
		("UnityEngine.Rendering.HighDefinition", "com.unity.render-pipelines.high-definition"),
		("HDRenderPipeline", "com.unity.render-pipelines.high-definition"),
		("using UnityEngine.InputSystem", "com.unity.inputsystem"),
		("namespace UnityEngine.InputSystem", "com.unity.inputsystem"),
		("using Unity.XR.Management", "com.unity.xr.management"),
		("using Unity.XR.Oculus", "com.unity.xr.oculus"),
		("using Unity.Netcode", "com.unity.netcode.gameobjects"),
		("namespace Unity.Netcode", "com.unity.netcode.gameobjects"),
	];

	[GeneratedRegex("\"((?:com|io)\\.[a-z0-9][a-z0-9\\.-]*)\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase)]
	private static partial Regex PackageWithVersionRegex();

	[GeneratedRegex("(?:com|io)\\.[a-z0-9][a-z0-9\\.-]*", RegexOptions.IgnoreCase)]
	private static partial Regex PackageNameRegex();

	[GeneratedRegex("^(?:com|io)\\.[a-z0-9][a-z0-9\\.-]*$", RegexOptions.IgnoreCase)]
	private static partial Regex PackageIdRegex();

	protected readonly record struct DetectedPackage(string? Version, PackageVersionSource Source);

	protected enum PackageVersionSource
	{
		Hint = 0,
		Assembly = 1,
		ExplicitText = 2,
		Manifest = 3,
		PackageLock = 4,
		DeepScan = 5,
	}
}
