using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.Primitives;
using AssetRipper.Processing.Extended.UnityPackages;

namespace AssetRipper.Processing.Extended.Tests;

public class ExtendedPackageManifestPostExporterTests
{
	private static readonly UnityVersion Version = new(6000, 0, 62, UnityVersionType.Final, 1);

	private static UnityPackageData Package(string name, string version, bool used) => new()
	{
		Name = name,
		Version = version,
		UsedInPackageJson = used,
		Assets = new AssetDictionary(),
	};

	[Test]
	public void IncludesPackagesFlaggedForPackageJson()
	{
		ExtendedPackageManifestPostExporter exporter = new([Package("com.unity.textmeshpro", "3.0.6", true)]);

		PackageManifest manifest = exporter.BuildManifest(Version);

		Assert.That(manifest.Dependencies["com.unity.textmeshpro"], Is.EqualTo("3.0.6"));
	}

	[Test]
	public void ExcludesPackagesNotFlaggedForPackageJson()
	{
		ExtendedPackageManifestPostExporter exporter = new([Package("com.example.internal", "1.0.0", false)]);

		PackageManifest manifest = exporter.BuildManifest(Version);

		Assert.That(manifest.Dependencies.ContainsKey("com.example.internal"), Is.False);
	}

	[Test]
	public void StillContainsTheDefaultUnityModules()
	{
		ExtendedPackageManifestPostExporter exporter = new([]);

		PackageManifest manifest = exporter.BuildManifest(Version);

		Assert.That(manifest.Dependencies.ContainsKey("com.unity.modules.animation"), Is.True);
	}

	[Test]
	public void DoesNotOverwriteADefaultModuleOfTheSameName()
	{
		ExtendedPackageManifestPostExporter exporter = new([Package("com.unity.modules.animation", "9.9.9", true)]);

		PackageManifest manifest = exporter.BuildManifest(Version);

		Assert.That(manifest.Dependencies["com.unity.modules.animation"], Is.EqualTo("1.0.0"));
	}
}
