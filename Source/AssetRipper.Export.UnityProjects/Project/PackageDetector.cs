using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Export.UnityProjects.Project;

internal static class PackageDetector
{
	/// <summary>
	/// Detect Unity packages used by the game based on loaded assemblies.
	/// Iterates <paramref name="assemblyManager"/> exactly once; <paramref name="packageAssemblyNames"/>
	/// receives every assembly that maps to a resolved package so the script exporter can mark them as
	/// Skip without re-walking the assembly list.
	/// </summary>
	public static Dictionary<string, string> Detect(
		IAssemblyManager assemblyManager,
		Dictionary<string, UnityGuid> referenceAssemblies,
		UnityVersion projectVersion,
		out Dictionary<string, string> scriptGuids,
		out HashSet<string> packageAssemblyNames)
	{
		scriptGuids = new Dictionary<string, string>(StringComparer.Ordinal);
		packageAssemblyNames = new HashSet<string>(StringComparer.Ordinal);

		if (!assemblyManager.IsSet)
		{
			return new Dictionary<string, string>(StringComparer.Ordinal);
		}

		Dictionary<string, List<string>> packageToAssemblies = new(StringComparer.Ordinal);

		foreach (AsmResolver.DotNet.AssemblyDefinition assembly in assemblyManager.GetAssemblies())
		{
			string? assemblyName = assembly.Name;
			if (string.IsNullOrEmpty(assemblyName))
			{
				continue;
			}

			if (ReferenceAssemblies.IsPredefinedAssembly(assemblyName))
			{
				continue;
			}

			if (referenceAssemblies.ContainsKey(assemblyName))
			{
				continue;
			}

			string? packageName = AssemblyToPackageMapper.TryGetPackageName(assemblyName);
			if (packageName is null)
			{
				continue;
			}

			if (!packageToAssemblies.TryGetValue(packageName, out List<string>? list))
			{
				list = new List<string>();
				packageToAssemblies[packageName] = list;
			}
			list.Add(assemblyName);
		}

		if (packageToAssemblies.Count == 0)
		{
			Logger.Info(LogCategory.Export, "No additional Unity packages detected.");
			return new Dictionary<string, string>(StringComparer.Ordinal);
		}

		Dictionary<string, string> detectedPackages = new(StringComparer.Ordinal);

		Logger.Info(LogCategory.Export, $"Detected {packageToAssemblies.Count} candidate package(s). Resolving versions from Unity Registry...");

		using UnityRegistryClient registryClient = new();

		foreach ((string packageName, List<string> assemblyNames) in packageToAssemblies)
		{
			PackageVersionInfo? info = registryClient.GetCompatibleVersionInfo(packageName, projectVersion);

			if (info is not null)
			{
				detectedPackages[packageName] = info.Version;
				Logger.Info(LogCategory.Export, $"  {packageName} @ {info.Version}");

				if (info.TarballUrl is not null)
				{
					Logger.Info(LogCategory.Export, $"  Extracting script GUIDs from {packageName}...");
					Dictionary<string, UnityGuid> packageGuids = registryClient.ExtractScriptGuids(info.TarballUrl);
					foreach (KeyValuePair<string, UnityGuid> kvp in packageGuids)
					{
						scriptGuids[kvp.Key] = kvp.Value.ToString();
					}
				}
			}
			else
			{
				string? fallbackVersion = AssemblyToPackageMapper.GetFallbackVersion(packageName);
				if (fallbackVersion is null)
				{
					Logger.Warning(LogCategory.Export, $"  {packageName} — skipped (not found on registry, likely a sub-assembly of another package)");
					continue;
				}

				detectedPackages[packageName] = fallbackVersion;
				Logger.Info(LogCategory.Export, $"  {packageName} @ {fallbackVersion} (embedded package, using fallback version)");

				// GUIDs are stable across all versions of a package, so any available version works for embedded packages.
				string? anyTarballUrl = registryClient.GetAnyTarballUrl(packageName);
				if (anyTarballUrl is not null)
				{
					Logger.Info(LogCategory.Export, $"  Extracting script GUIDs from {packageName} (using any available version)...");
					Dictionary<string, UnityGuid> packageGuids = registryClient.ExtractScriptGuids(anyTarballUrl);
					foreach (KeyValuePair<string, UnityGuid> kvp in packageGuids)
					{
						scriptGuids[kvp.Key] = kvp.Value.ToString();
					}
				}
			}

			foreach (string assemblyName in assemblyNames)
			{
				packageAssemblyNames.Add(assemblyName);
			}
		}

		Logger.Info(LogCategory.Export, $"Resolved {detectedPackages.Count} Unity package(s) with {scriptGuids.Count} script GUID(s).");
		return detectedPackages;
	}
}
