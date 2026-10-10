using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects.Project;

internal static class PackageDetector
{
	/// <summary>
	/// Detect Unity packages used by the game based on loaded assemblies. The assembly manager is
	/// iterated exactly once; assemblies that resolve to a package are reported in
	/// <see cref="PackageDetectionResult.PackageAssemblies"/> so the script exporter can mark them as
	/// Skip without re-walking the assembly list.
	/// </summary>
	public static PackageDetectionResult Detect(
		IAssemblyManager assemblyManager,
		Dictionary<string, UnityGuid> referenceAssemblies,
		UnityVersion projectVersion)
	{
		if (!assemblyManager.IsSet)
		{
			return PackageDetectionResult.Empty;
		}

		Dictionary<string, List<string>> packageToAssemblies = CollectCandidates(assemblyManager, referenceAssemblies);

		if (packageToAssemblies.Count == 0)
		{
			Logger.Info(LogCategory.Export, "No additional Unity packages detected.");
			return PackageDetectionResult.Empty;
		}

		Logger.Info(LogCategory.Export, $"Detected {packageToAssemblies.Count} candidate package(s). Resolving versions from Unity Registry...");

		Dictionary<string, string> detectedPackages = new(StringComparer.Ordinal);
		Dictionary<string, UnityGuid> scriptGuids = new(StringComparer.Ordinal);
		HashSet<string> packageAssemblyNames = new(StringComparer.Ordinal);

		using UnityRegistryClient registryClient = new();

		foreach ((string packageName, List<string> assemblyNames) in packageToAssemblies)
		{
			if (TryResolvePackage(registryClient, packageName, projectVersion, scriptGuids, out string? resolvedVersion))
			{
				detectedPackages[packageName] = resolvedVersion;
				packageAssemblyNames.UnionWith(assemblyNames);
			}
		}

		Logger.Info(LogCategory.Export, $"Resolved {detectedPackages.Count} Unity package(s) with {scriptGuids.Count} script GUID(s).");
		return new PackageDetectionResult(detectedPackages, scriptGuids, packageAssemblyNames);
	}

	private static Dictionary<string, List<string>> CollectCandidates(
		IAssemblyManager assemblyManager,
		Dictionary<string, UnityGuid> referenceAssemblies)
	{
		Dictionary<string, List<string>> result = new(StringComparer.Ordinal);

		IEnumerable<string> candidates = assemblyManager.GetAssemblies()
			.Select(assembly => (string?)assembly.Name)
			.Where(name => !string.IsNullOrEmpty(name)
				&& !ReferenceAssemblies.IsPredefinedAssembly(name)
				&& !referenceAssemblies.ContainsKey(name))
			.Select(name => name!);

		foreach (string assemblyName in candidates)
		{
			string? packageName = AssemblyToPackageMapper.TryGetPackageName(assemblyName);
			if (packageName is null)
			{
				continue;
			}

			if (!result.TryGetValue(packageName, out List<string>? list))
			{
				list = new List<string>();
				result[packageName] = list;
			}
			list.Add(assemblyName);
		}

		return result;
	}

	private static bool TryResolvePackage(
		UnityRegistryClient registryClient,
		string packageName,
		UnityVersion projectVersion,
		Dictionary<string, UnityGuid> scriptGuids,
		[NotNullWhen(true)] out string? resolvedVersion)
	{
		PackageVersionInfo? info = registryClient.GetCompatibleVersionInfo(packageName, projectVersion);
		if (info is not null)
		{
			resolvedVersion = info.Version;
			Logger.Info(LogCategory.Export, $"  {packageName} @ {info.Version}");
			if (info.TarballUrl is not null)
			{
				Logger.Info(LogCategory.Export, $"  Extracting script GUIDs from {packageName}...");
				MergeGuids(registryClient.ExtractScriptGuids(info.TarballUrl), scriptGuids);
			}
			return true;
		}

		string? fallback = AssemblyToPackageMapper.GetFallbackVersion(packageName);
		if (fallback is null)
		{
			Logger.Warning(LogCategory.Export, $"  {packageName} — skipped (not found on registry, likely a sub-assembly of another package)");
			resolvedVersion = null;
			return false;
		}

		resolvedVersion = fallback;
		Logger.Info(LogCategory.Export, $"  {packageName} @ {fallback} (embedded package, using fallback version)");

		// GUIDs are stable across all versions of a package, so any available version works for embedded packages.
		string? anyTarballUrl = registryClient.GetAnyTarballUrl(packageName);
		if (anyTarballUrl is not null)
		{
			Logger.Info(LogCategory.Export, $"  Extracting script GUIDs from {packageName} (using any available version)...");
			MergeGuids(registryClient.ExtractScriptGuids(anyTarballUrl), scriptGuids);
		}
		return true;
	}

	private static void MergeGuids(Dictionary<string, UnityGuid> source, Dictionary<string, UnityGuid> target)
	{
		foreach (KeyValuePair<string, UnityGuid> kvp in source)
		{
			target[kvp.Key] = kvp.Value;
		}
	}
}
