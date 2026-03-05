using AssetRipper.Export.Configuration;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using System.IO;
using System.Text.Json;

namespace AssetRipper.Export.UnityProjects.Project;

public class PackageManifestPostExporter : IPostExporter
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
		ScanExportedScriptsAndShaders(settings.AssetsPath, fileSystem, dependencies);
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

	private static void ScanExportedScriptsAndShaders(string assetsPath, FileSystem fileSystem, Dictionary<string, string?> dependencies)
	{
		if (!fileSystem.Directory.Exists(assetsPath))
		{
			return;
		}

		foreach (string scriptPath in fileSystem.Directory.EnumerateFiles(assetsPath, "*.cs", SearchOption.AllDirectories))
		{
			string text = TryReadAllText(fileSystem, scriptPath);
			if (text.Length == 0)
			{
				continue;
			}

			if (ContainsAny(text, "namespace UnityEngine.UI", "using UnityEngine.UI"))
			{
				dependencies.TryAdd("com.unity.ugui", null);
			}
			if (ContainsAny(text, "using TMPro", "namespace TMPro"))
			{
				dependencies.TryAdd("com.unity.textmeshpro", null);
			}
			if (ContainsAny(text, "using Unity.Entities", "namespace Unity.Entities"))
			{
				dependencies.TryAdd("com.unity.entities", null);
			}
			if (ContainsAny(text, "UnityEngine.Rendering.Universal", "Universal Render Pipeline"))
			{
				dependencies.TryAdd("com.unity.render-pipelines.universal", null);
			}
		}

		foreach (string shaderPath in fileSystem.Directory.EnumerateFiles(assetsPath, "*.shader", SearchOption.AllDirectories))
		{
			string text = TryReadAllText(fileSystem, shaderPath);
			if (ContainsAny(text, "Universal Render Pipeline", "RenderPipeline\"=\"UniversalPipeline"))
			{
				dependencies.TryAdd("com.unity.render-pipelines.universal", null);
			}
			if (ContainsAny(text, "TextMeshPro", "TMP_"))
			{
				dependencies.TryAdd("com.unity.textmeshpro", null);
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

	private static bool ContainsAny(string text, params string[] values)
	{
		foreach (string value in values)
		{
			if (text.Contains(value, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}
}