using AssetRipper.Export.Configuration;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AssetRipper.Export.UnityProjects.Project;

public partial class PackageManifestPostExporter : IPostExporter
{
	public void DoPostExport(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		string packagesDirectory = fileSystem.Path.Join(settings.ProjectRootPath, "Packages");
		fileSystem.Directory.Create(packagesDirectory);

		PackageManifest manifest = CreateManifest(settings.Version);
		Dictionary<string, string?> discoveredPackages = ScanDependencies(gameData, settings, fileSystem);
		InjectDiscoveredDependencies(manifest, settings.Version, discoveredPackages);

		string path = fileSystem.Path.Join(packagesDirectory, "manifest.json");
		using Stream stream = fileSystem.File.Create(path);
		manifest.Save(stream);
	}

	protected virtual PackageManifest CreateManifest(UnityVersion version)
	{
		return PackageManifest.CreateDefault(version);
	}

	protected virtual Dictionary<string, string?> ScanDependencies(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		Dictionary<string, string?> dependencies = new(StringComparer.Ordinal);

		ReadEmbeddedManifestDependencies(gameData, dependencies);
		ReadEmbeddedPackageLockDependencies(gameData, dependencies);
		ScanExportedAssets(settings.AssetsPath, fileSystem, dependencies);
		ScanAssemblyNames(gameData, dependencies);

		return dependencies;
	}

	protected virtual void InjectDiscoveredDependencies(PackageManifest manifest, UnityVersion version, Dictionary<string, string?> discoveredPackages)
	{
		foreach ((string packageName, string? explicitVersion) in discoveredPackages)
		{
			if (!string.IsNullOrWhiteSpace(explicitVersion))
			{
				manifest.Dependencies[packageName] = explicitVersion;
			}
			else
			{
				manifest.Dependencies[packageName] = ResolveVersion(packageName, version);
			}
		}
	}

	private static void ReadEmbeddedManifestDependencies(GameData gameData, Dictionary<string, string?> dependencies)
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
							dependencies[property.Name] = property.Value.GetString();
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

	private static void ReadEmbeddedPackageLockDependencies(GameData gameData, Dictionary<string, string?> dependencies)
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
							dependencies[property.Name] = versionElement.GetString();
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

	private static void ScanExportedAssets(string assetsPath, FileSystem fileSystem, Dictionary<string, string?> dependencies)
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

	private static void ScanAssemblyNames(GameData gameData, Dictionary<string, string?> dependencies)
	{
		foreach (var assembly in gameData.AssemblyManager.GetAssemblies())
		{
			string assemblyName = assembly.Name ?? string.Empty;
			if (assemblyName.Length == 0)
			{
				continue;
			}

			if (assemblyName.StartsWith("UnityEngine.UI", StringComparison.Ordinal)
				|| assemblyName.StartsWith("Unity.UI", StringComparison.Ordinal))
			{
				dependencies.TryAdd("com.unity.ugui", null);
			}
			if (assemblyName.StartsWith("Unity.TextMeshPro", StringComparison.Ordinal)
				|| assemblyName.StartsWith("UnityEngine.TextCoreTextEngine", StringComparison.Ordinal))
			{
				dependencies.TryAdd("com.unity.textmeshpro", null);
			}
			if (assemblyName.StartsWith("Unity.Entities", StringComparison.Ordinal))
			{
				dependencies.TryAdd("com.unity.entities", null);
			}
			if (assemblyName.StartsWith("Unity.RenderPipelines.Universal", StringComparison.Ordinal)
				|| assemblyName.StartsWith("UnityEngine.Rendering.Universal", StringComparison.Ordinal))
			{
				dependencies.TryAdd("com.unity.render-pipelines.universal", null);
			}
			if (assemblyName.StartsWith("Unity.RenderPipelines.HighDefinition", StringComparison.Ordinal)
				|| assemblyName.StartsWith("UnityEngine.Rendering.HighDefinition", StringComparison.Ordinal))
			{
				dependencies.TryAdd("com.unity.render-pipelines.high-definition", null);
			}
			if (assemblyName.StartsWith("Unity.RenderPipelines.Core", StringComparison.Ordinal))
			{
				dependencies.TryAdd("com.unity.render-pipelines.core", null);
			}
			if (assemblyName.StartsWith("Unity.InputSystem", StringComparison.Ordinal)
				|| assemblyName.StartsWith("UnityEngine.InputSystem", StringComparison.Ordinal))
			{
				dependencies.TryAdd("com.unity.inputsystem", null);
			}
			if (assemblyName.StartsWith("Unity.XR.Management", StringComparison.Ordinal))
			{
				dependencies.TryAdd("com.unity.xr.management", null);
			}
			if (assemblyName.StartsWith("Unity.XR.Oculus", StringComparison.Ordinal))
			{
				dependencies.TryAdd("com.unity.xr.oculus", null);
			}
			if (assemblyName.StartsWith("Unity.Netcode", StringComparison.Ordinal))
			{
				dependencies.TryAdd("com.unity.netcode.gameobjects", null);
			}
		}
	}

	private static void AddPackageHintsFromText(string text, Dictionary<string, string?> dependencies)
	{
		foreach ((string token, string package) in PackageTokenHints)
		{
			if (text.Contains(token, StringComparison.Ordinal))
			{
				dependencies.TryAdd(package, null);
			}
		}
	}

	private static void AddExplicitPackagesFromText(string text, Dictionary<string, string?> dependencies)
	{
		foreach (Match match in PackageWithVersionRegex().Matches(text))
		{
			string packageName = match.Groups[1].Value;
			string version = match.Groups[2].Value;
			if (packageName.StartsWith("com.unity.", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(version))
			{
				dependencies[packageName] = version;
			}
		}

		foreach (Match match in PackageNameRegex().Matches(text))
		{
			string packageName = match.Value;
			if (packageName.StartsWith("com.unity.", StringComparison.Ordinal))
			{
				dependencies.TryAdd(packageName, null);
			}
		}
	}

	private static string ResolveVersion(string packageName, UnityVersion version)
	{
		return packageName switch
		{
			"com.unity.ugui" => "1.0.0",
			"com.unity.textmeshpro" => version.GreaterThanOrEquals(2021) ? "3.0.6" : version.GreaterThanOrEquals(2019) ? "2.1.6" : "1.5.0",
			"com.unity.entities" => version.GreaterThanOrEquals(2022) ? "1.0.0" : version.GreaterThanOrEquals(2020) ? "0.51.1-preview.21" : "0.17.0-preview.42",
			"com.unity.render-pipelines.universal" => version.GreaterThanOrEquals(2021) ? "12.0.0" : version.GreaterThanOrEquals(2020) ? "10.0.0" : "7.5.3",
			"com.unity.render-pipelines.high-definition" => version.GreaterThanOrEquals(2021) ? "12.1.0" : version.GreaterThanOrEquals(2020) ? "10.5.1" : "7.5.3",
			"com.unity.render-pipelines.core" => version.GreaterThanOrEquals(2021) ? "12.0.0" : version.GreaterThanOrEquals(2020) ? "10.0.0" : "7.5.3",
			"com.unity.inputsystem" => version.GreaterThanOrEquals(2021) ? "1.5.1" : version.GreaterThanOrEquals(2020) ? "1.4.4" : "1.0.2",
			"com.unity.xr.management" => version.GreaterThanOrEquals(2021) ? "4.2.0" : "4.0.1",
			"com.unity.xr.oculus" => version.GreaterThanOrEquals(2021) ? "4.3.0" : "3.2.3",
			"com.unity.netcode.gameobjects" => version.GreaterThanOrEquals(2022) ? "1.5.2" : "1.0.0",
			_ => "1.0.0",
		};
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

	[GeneratedRegex("\"(com\\.unity\\.[a-z0-9][a-z0-9\\.-]*)\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase)]
	private static partial Regex PackageWithVersionRegex();

	[GeneratedRegex("com\\.unity\\.[a-z0-9][a-z0-9\\.-]*", RegexOptions.IgnoreCase)]
	private static partial Regex PackageNameRegex();
}
