using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Primitives;

namespace AssetRipper.Tests;

public class PackageManifestTests
{
	[TestCase("6000.3.25f1", true)]
	[TestCase("6000.5.1f1", false)]
	[TestCase("6000.5.10f1", false)]
	public void LegacyVrModuleIsOnlyRequestedBeforeUnity65(string version, bool expected)
	{
		PackageManifest manifest = PackageManifest.CreateDefault(UnityVersion.Parse(version));
		Assert.That(manifest.Dependencies.ContainsKey("com.unity.modules.vr"), Is.EqualTo(expected));
		Assert.That(manifest.Dependencies.ContainsKey("com.unity.modules.xr"), Is.True);
	}
}
