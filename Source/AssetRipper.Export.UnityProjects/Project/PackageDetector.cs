using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Export.UnityProjects.Project;

/// <summary>
/// Detects Unity packages from loaded assemblies by mapping assembly names to package names.
/// Uses the Unity Package Registry to resolve correct versions.
/// </summary>
public static class PackageDetector
{
	private const string FallbackVersion = "0.0.0";

	/// <summary>
	/// Detect Unity packages used by the game based on loaded assemblies.
	/// Queries the Unity Package Registry to resolve correct versions and extract assembly GUIDs.
	/// </summary>
	/// <param name="assemblyManager">The assembly manager with loaded game assemblies.</param>
	/// <param name="referenceAssemblies">Already known reference assemblies (framework/Unity standard).</param>
	/// <param name="projectVersion">The Unity version of the project, used for version compatibility.</param>
	/// <param name="scriptGuids">Output: class name → .cs.meta GUID hex string mapping extracted from package tarballs.</param>
	/// <returns>Dictionary of detected package names to their versions.</returns>
	public static Dictionary<string, string> Detect(
		IAssemblyManager assemblyManager,
		Dictionary<string, UnityGuid> referenceAssemblies,
		UnityVersion projectVersion,
		out Dictionary<string, string> scriptGuids)
	{
		scriptGuids = new Dictionary<string, string>(StringComparer.Ordinal);

		// First pass: identify which packages are used
		HashSet<string> candidatePackages = new(StringComparer.Ordinal);

		if (!assemblyManager.IsSet)
		{
			return new Dictionary<string, string>(StringComparer.Ordinal);
		}

		foreach (AsmResolver.DotNet.AssemblyDefinition assembly in assemblyManager.GetAssemblies())
		{
			string? assemblyName = assembly.Name;
			if (string.IsNullOrEmpty(assemblyName))
			{
				continue;
			}

			// Skip predefined assemblies (Assembly-CSharp, etc.)
			if (ReferenceAssemblies.IsPredefinedAssembly(assemblyName))
			{
				continue;
			}

			// Skip assemblies already known as framework/Unity standard
			if (referenceAssemblies.ContainsKey(assemblyName))
			{
				continue;
			}

			// Try to map assembly to a package
			string? packageName = AssemblyToPackageMapper.TryGetPackageName(assemblyName);
			if (packageName is not null)
			{
				candidatePackages.Add(packageName);
			}
		}

		if (candidatePackages.Count == 0)
		{
			Logger.Info(LogCategory.Export, "No additional Unity packages detected.");
			return new Dictionary<string, string>(StringComparer.Ordinal);
		}

		// Second pass: resolve versions and extract GUIDs via Unity Registry API
		Dictionary<string, string> detectedPackages = new(StringComparer.Ordinal);

		Logger.Info(LogCategory.Export, $"Detected {candidatePackages.Count} candidate package(s). Resolving versions from Unity Registry...");

		using UnityRegistryClient registryClient = new();

		foreach (string packageName in candidatePackages)
		{
			PackageVersionInfo? info = registryClient.GetCompatibleVersionInfo(packageName, projectVersion);

			if (info is not null)
			{
				detectedPackages[packageName] = info.Version;
				Logger.Info(LogCategory.Export, $"  {packageName} @ {info.Version}");

				// Extract per-script .cs.meta GUIDs from the package tarball
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
				// Try static fallback for embedded packages (e.g. com.unity.ugui)
				string? fallbackVersion = AssemblyToPackageMapper.GetFallbackVersion(packageName);
				if (fallbackVersion is not null)
				{
					detectedPackages[packageName] = fallbackVersion;
					Logger.Info(LogCategory.Export, $"  {packageName} @ {fallbackVersion} (embedded package, using fallback version)");

					// Still try to extract GUIDs from any available version on the registry.
					// GUIDs are stable across all versions of a package.
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
				else
				{
					// Package not found on registry and no fallback — skip it entirely.
					Logger.Warning(LogCategory.Export, $"  {packageName} — skipped (not found on registry, likely a sub-assembly of another package)");
				}
			}
		}

		Logger.Info(LogCategory.Export, $"Resolved {detectedPackages.Count} Unity package(s) with {scriptGuids.Count} script GUID(s).");
		return detectedPackages;
	}

	/// <summary>
	/// Get all assembly names that belong to detected packages.
	/// Used to add them to the reference assembly dictionary so they get skipped during export.
	/// </summary>
	public static HashSet<string> GetPackageAssemblyNames(
		IAssemblyManager assemblyManager,
		Dictionary<string, string> detectedPackages)
	{
		HashSet<string> packageAssemblyNames = new(StringComparer.Ordinal);

		if (!assemblyManager.IsSet)
		{
			return packageAssemblyNames;
		}

		foreach (AsmResolver.DotNet.AssemblyDefinition assembly in assemblyManager.GetAssemblies())
		{
			string? assemblyName = assembly.Name;
			if (string.IsNullOrEmpty(assemblyName))
			{
				continue;
			}

			string? packageName = AssemblyToPackageMapper.TryGetPackageName(assemblyName);
			if (packageName is not null && detectedPackages.ContainsKey(packageName))
			{
				packageAssemblyNames.Add(assemblyName);
			}
		}

		return packageAssemblyNames;
	}
}
