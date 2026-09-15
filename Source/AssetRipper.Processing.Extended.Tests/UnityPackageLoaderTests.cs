using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.Processing.Extended.UnityPackages;
using System.Text.Json;

namespace AssetRipper.Processing.Extended.Tests;

public class UnityPackageLoaderTests
{
	private static string ToJson(string name, string version, bool usedInPackageJson)
	{
		UnityPackageData package = new()
		{
			Name = name,
			Version = version,
			UsedInPackageJson = usedInPackageJson,
			Assets = new AssetDictionary(),
		};
		return JsonSerializer.Serialize(package, MiningSerializerContext.Default.UnityPackageData);
	}

	[Test]
	public void LoadsWellFormedPackages()
	{
		List<UnityPackageData> packages = UnityPackageLoader.Load(
		[
			ToJson("com.unity.textmeshpro", "3.0.6", true),
			ToJson("com.unity.burst", "1.8.0", false),
		]);

		Assert.That(packages, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{
			Assert.That(packages[0].Name, Is.EqualTo("com.unity.textmeshpro"));
			Assert.That(packages[0].Version, Is.EqualTo("3.0.6"));
			Assert.That(packages[0].UsedInPackageJson, Is.True);
			Assert.That(packages[1].UsedInPackageJson, Is.False);
		});
	}

	[Test]
	public void SkipsMalformedJsonWithoutThrowing()
	{
		List<UnityPackageData> packages = null!;

		Assert.DoesNotThrow(() => packages = UnityPackageLoader.Load(["{ this is not json", ToJson("com.unity.burst", "1.8.0", true)]));
		Assert.That(packages, Has.Count.EqualTo(1));
		Assert.That(packages[0].Name, Is.EqualTo("com.unity.burst"));
	}

	[Test]
	public void SkipsPackagesWithNoName()
	{
		List<UnityPackageData> packages = UnityPackageLoader.Load([ToJson("", "1.0.0", true)]);

		Assert.That(packages, Is.Empty);
	}

	[Test]
	public void PackageJsonWithoutAnAssetsKeyIsSafeToEnumerate()
	{
		// Assets is a struct; when the key is absent it stays default and even reading
		// Count throws. The loader must normalize it.
		List<UnityPackageData> packages = UnityPackageLoader.Load(["""{"Name":"com.example.minimal","Version":"1.0.0"}"""]);

		Assert.That(packages, Has.Count.EqualTo(1));
		Assert.DoesNotThrow(() => _ = packages[0].Assets.Count);
		Assert.That(packages[0].Assets.Count, Is.Zero);
	}

	[Test]
	public void EmptyInputYieldsEmptyList()
	{
		Assert.That(UnityPackageLoader.Load([]), Is.Empty);
	}
}
