namespace AssetRipper.Processing;

/// <summary>
/// Result of automatic Unity-package detection for a <see cref="GameData"/> instance.
/// </summary>
/// <param name="Packages">Detected Unity packages keyed by package name (e.g. <c>com.unity.timeline</c>), value is the resolved version.</param>
/// <param name="ScriptGuids">Per-class <c>.cs.meta</c> GUIDs harvested from package tarballs, keyed by class name.</param>
/// <param name="PackageAssemblies">Assembly names that belong to a detected package and must therefore be skipped by the script exporter.</param>
public sealed record PackageDetectionResult(
	IReadOnlyDictionary<string, string> Packages,
	IReadOnlyDictionary<string, UnityGuid> ScriptGuids,
	IReadOnlySet<string> PackageAssemblies)
{
	public static PackageDetectionResult Empty { get; } = new(
		new Dictionary<string, string>(StringComparer.Ordinal),
		new Dictionary<string, UnityGuid>(StringComparer.Ordinal),
		new HashSet<string>(StringComparer.Ordinal));
}
