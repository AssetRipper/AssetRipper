using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Import.Logging;
using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.Primitives;

namespace AssetRipper.Processing.Extended.UnityPackages;

/// <summary>
/// Adds user-supplied packages to the exported project's manifest so Unity resolves
/// them instead of the exported copies.
/// </summary>
public sealed class ExtendedPackageManifestPostExporter : PackageManifestPostExporter
{
	private readonly IReadOnlyList<UnityPackageData> packages;

	public ExtendedPackageManifestPostExporter(IReadOnlyList<UnityPackageData> packages)
	{
		ArgumentNullException.ThrowIfNull(packages);
		this.packages = packages;
	}

	/// <summary>
	/// Exposed for testing. <see cref="CreateManifest"/> is protected on the base type.
	/// </summary>
	public PackageManifest BuildManifest(UnityVersion version) => CreateManifest(version);

	protected override PackageManifest CreateManifest(UnityVersion version)
	{
		PackageManifest manifest = base.CreateManifest(version);
		foreach (UnityPackageData package in packages)
		{
			if (!package.UsedInPackageJson)
			{
				continue;
			}

			// TryAdd, not assignment: a real Unity module already in the defaults wins.
			if (manifest.Dependencies.TryAdd(package.Name, package.Version))
			{
				Logger.Info(LogCategory.Export, $"Manifest: added {package.Name} {package.Version}.");
			}
		}
		return manifest;
	}
}
