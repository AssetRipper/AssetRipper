namespace AssetRipper.Export.Configuration;

public enum PackageDetectionMode
{
	/// <summary>
	/// No automatic package detection. All assemblies are handled normally.
	/// </summary>
	Off,
	/// <summary>
	/// Automatically detect Unity packages from loaded assemblies and add them to the project manifest.
	/// </summary>
	Auto,
}
