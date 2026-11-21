using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Primitives;
using AssetRipper.Processing;
using System.Text.Json;

namespace AssetRipper.Tests;

internal class PackageDetectionTests
{
	#region AssemblyToPackageMapper

	[TestCase("UnityEngine.UI", "com.unity.ugui")]
	[TestCase("UnityEditor.UI", "com.unity.ugui")]
	[TestCase("Unity.TextMeshPro", "com.unity.textmeshpro")]
	[TestCase("Unity.TextMeshPro.Editor", "com.unity.textmeshpro")]
	[TestCase("Unity.Timeline", "com.unity.timeline")]
	[TestCase("Unity.Timeline.Editor", "com.unity.timeline")]
	[TestCase("Unity.InputSystem", "com.unity.inputsystem")]
	[TestCase("Unity.Cinemachine", "com.unity.cinemachine")]
	[TestCase("Cinemachine", "com.unity.cinemachine")]
	[TestCase("Unity.ProBuilder", "com.unity.probuilder")]
	[TestCase("Unity.ProBuilder.KdTree", "com.unity.probuilder")]
	[TestCase("Unity.ProBuilder.Poly2Tri", "com.unity.probuilder")]
	[TestCase("Newtonsoft.Json", "com.unity.nuget.newtonsoft-json")]
	[TestCase("nunit.framework", "com.unity.ext.nunit")]
	[TestCase("Unity.Postprocessing.Runtime", "com.unity.postprocessing")]
	[TestCase("Unity.RenderPipelines.Universal.Runtime", "com.unity.render-pipelines.universal")]
	[TestCase("Unity.XR.OpenXR.Features.ConformanceAutomation", "com.unity.xr.openxr")]
	[TestCase("Unity.XR.OpenXR.Features.MockRuntime", "com.unity.xr.openxr")]
	[TestCase("Unity.XR.OpenXR.Features.RuntimeDebugger", "com.unity.xr.openxr")]
	[TestCase("Unity.XR.CoreUtils", "com.unity.xr.core-utils")]
	[TestCase("Unity.XR.ARFoundation.InternalUtils", "com.unity.xr.arfoundation")]
	[TestCase("Unity.XR.Simulation", "com.unity.xr.arfoundation")]
	[TestCase("UnityEngine.SpatialTracking", "com.unity.xr.legacyinputhelpers")]
	[TestCase("Unity.XR.Interaction.Toolkit.Samples.SpatialKeyboard", "com.unity.xr.interaction.toolkit")]
	public void StaticMapping_ReturnsCorrectPackage(string assemblyName, string expectedPackage)
	{
		string? result = AssemblyToPackageMapper.TryGetPackageName(assemblyName);
		Assert.That(result, Is.EqualTo(expectedPackage));
	}

	[TestCase("Assembly-CSharp")]
	[TestCase("mscorlib")]
	[TestCase("System.Core")]
	[TestCase("SomeThirdPartyLib")]
	public void StaticMapping_ReturnsNull_ForNonPackageAssemblies(string assemblyName)
	{
		string? result = AssemblyToPackageMapper.TryGetPackageName(assemblyName);
		Assert.That(result, Is.Null);
	}

	[TestCase("Unity.SomeNewPackage", "com.unity.somenewpackage")]
	[TestCase("Unity.SomeNewPackage.Editor", "com.unity.somenewpackage")]
	[TestCase("Unity.SomeNewPackage.Runtime", "com.unity.somenewpackage")]
	[TestCase("Unity.SomeNewPackage.Tests", "com.unity.somenewpackage")]
	public void HeuristicMapping_DerivePackageFromAssemblyName(string assemblyName, string expectedPackage)
	{
		string? result = AssemblyToPackageMapper.TryGetPackageName(assemblyName);
		Assert.That(result, Is.EqualTo(expectedPackage));
	}

	[TestCase("SomeLib.Unity.Something")]
	[TestCase("MyCompany.MyAssembly")]
	public void HeuristicMapping_ReturnsNull_ForNonUnityPrefix(string assemblyName)
	{
		string? result = AssemblyToPackageMapper.TryGetPackageName(assemblyName);
		Assert.That(result, Is.Null);
	}

	[TestCase("com.unity.ugui", "1.0.0")]
	[TestCase("com.unity.2d.sprite", "1.0.0")]
	[TestCase("com.unity.2d.tilemap", "1.0.0")]
	public void GetFallbackVersion_ReturnsVersion_ForEmbeddedPackages(string packageName, string expectedVersion)
	{
		string? result = AssemblyToPackageMapper.GetFallbackVersion(packageName);
		Assert.That(result, Is.EqualTo(expectedVersion));
	}

	[TestCase("com.unity.timeline")]
	[TestCase("com.unity.inputsystem")]
	[TestCase("com.unity.nonexistent")]
	public void GetFallbackVersion_ReturnsNull_ForNonEmbeddedPackages(string packageName)
	{
		string? result = AssemblyToPackageMapper.GetFallbackVersion(packageName);
		Assert.That(result, Is.Null);
	}

	[Test]
	public void AllSubAssemblies_MapToSamePackage()
	{
		// ProBuilder sub-assemblies should all map to the same package
		string[] proBuilderAssemblies =
		[
			"Unity.ProBuilder",
			"Unity.ProBuilder.Editor",
			"Unity.ProBuilder.KdTree",
			"Unity.ProBuilder.Poly2Tri",
			"Unity.ProBuilder.Stl",
			"Unity.ProBuilder.Csg",
		];

		string[] results = proBuilderAssemblies
			.Select(a => AssemblyToPackageMapper.TryGetPackageName(a)!)
			.Distinct()
			.ToArray();

		Assert.That(results, Has.Length.EqualTo(1));
		Assert.That(results[0], Is.EqualTo("com.unity.probuilder"));
	}

	#endregion

	#region PackageManifest

	[Test]
	public void AddDetectedPackages_AddsToManifest()
	{
		PackageManifest manifest = new();
		Dictionary<string, string> packages = new()
		{
			["com.unity.textmeshpro"] = "3.0.6",
			["com.unity.timeline"] = "1.7.6",
		};

		manifest.AddDetectedPackages(packages);

		Assert.That(manifest.Dependencies, Does.ContainKey("com.unity.textmeshpro"));
		Assert.That(manifest.Dependencies["com.unity.textmeshpro"], Is.EqualTo("3.0.6"));
		Assert.That(manifest.Dependencies, Does.ContainKey("com.unity.timeline"));
		Assert.That(manifest.Dependencies["com.unity.timeline"], Is.EqualTo("1.7.6"));
	}

	[Test]
	public void AddDetectedPackages_DoesNotOverwriteExisting()
	{
		PackageManifest manifest = new();
		manifest.Dependencies["com.unity.textmeshpro"] = "2.0.0";

		Dictionary<string, string> packages = new()
		{
			["com.unity.textmeshpro"] = "3.0.6",
		};

		manifest.AddDetectedPackages(packages);

		Assert.That(manifest.Dependencies["com.unity.textmeshpro"], Is.EqualTo("2.0.0"));
	}

	[Test]
	public void CreateDefault_WithDetectedPackages_IncludesBoth()
	{
		PackageDetectionResult detected = new(
			new Dictionary<string, string> { ["com.unity.textmeshpro"] = "3.0.6" },
			new Dictionary<string, UnityGuid>(),
			new HashSet<string>());

		PackageManifest manifest = PackageManifest.CreateDefault(new UnityVersion(2022), detected);

		// Should have default modules
		Assert.That(manifest.Dependencies, Does.ContainKey("com.unity.modules.ui"));
		// Should also have detected package
		Assert.That(manifest.Dependencies, Does.ContainKey("com.unity.textmeshpro"));
		Assert.That(manifest.Dependencies["com.unity.textmeshpro"], Is.EqualTo("3.0.6"));
	}

	[Test]
	public void CreateDefault_WithoutDetectedPackages_OnlyDefaults()
	{
		PackageManifest manifest = PackageManifest.CreateDefault(new UnityVersion(2022));

		Assert.That(manifest.Dependencies, Does.ContainKey("com.unity.modules.ui"));
		Assert.That(manifest.Dependencies, Does.Not.ContainKey("com.unity.textmeshpro"));
	}

	[Test]
	public void Manifest_SerializesToValidJson()
	{
		PackageDetectionResult detected = new(
			new Dictionary<string, string> { ["com.unity.textmeshpro"] = "3.0.6" },
			new Dictionary<string, UnityGuid>(),
			new HashSet<string>());

		PackageManifest manifest = PackageManifest.CreateDefault(new UnityVersion(2022), detected);

		using MemoryStream stream = new();
		manifest.Save(stream);
		stream.Position = 0;

		using JsonDocument doc = JsonDocument.Parse(stream);
		Assert.That(doc.RootElement.TryGetProperty("dependencies", out JsonElement deps), Is.True);
		Assert.That(deps.TryGetProperty("com.unity.textmeshpro", out JsonElement tmpVersion), Is.True);
		Assert.That(tmpVersion.GetString(), Is.EqualTo("3.0.6"));
	}

	#endregion

	#region UnityRegistryClient - ParseGuidFromMeta

	[Test]
	public void ParseGuidFromMeta_ValidMeta_ReturnsGuid()
	{
		string metaContent = """
			fileFormatVersion: 2
			guid: f4688fdb7df04437aeb418b961361dc5
			MonoImporter:
			  serializedVersion: 2
			""";

		UnityGuid? result = UnityRegistryClient.ParseGuidFromMeta(metaContent);

		Assert.That(result, Is.Not.Null);
		Assert.That(result.ToString(), Is.EqualTo("f4688fdb7df04437aeb418b961361dc5"));
	}

	[Test]
	public void ParseGuidFromMeta_NoGuidLine_ReturnsNull()
	{
		string metaContent = """
			fileFormatVersion: 2
			MonoImporter:
			  serializedVersion: 2
			""";

		UnityGuid? result = UnityRegistryClient.ParseGuidFromMeta(metaContent);

		Assert.That(result, Is.Null);
	}

	[Test]
	public void ParseGuidFromMeta_InvalidGuidLength_ReturnsNull()
	{
		string metaContent = """
			fileFormatVersion: 2
			guid: abc123
			""";

		UnityGuid? result = UnityRegistryClient.ParseGuidFromMeta(metaContent);

		Assert.That(result, Is.Null);
	}

	[Test]
	public void ParseGuidFromMeta_EmptyContent_ReturnsNull()
	{
		UnityGuid? result = UnityRegistryClient.ParseGuidFromMeta("");

		Assert.That(result, Is.Null);
	}

	#endregion

	#region UnityRegistryClient - CompareVersionStrings

	[TestCase("2.0.0", "1.0.0", 1)]
	[TestCase("1.0.0", "2.0.0", -1)]
	[TestCase("1.0.0", "1.0.0", 0)]
	[TestCase("1.1.0", "1.0.0", 1)]
	[TestCase("1.0.1", "1.0.0", 1)]
	[TestCase("10.0.0", "9.0.0", 1)]
	[TestCase("3.0.6", "2.1.6", 1)]
	public void CompareVersionStrings_ComparesCorrectly(string a, string b, int expectedSign)
	{
		int result = UnityRegistryClient.CompareVersionStrings(a, b);

		if (expectedSign > 0)
		{
			Assert.That(result, Is.GreaterThan(0));
		}
		else if (expectedSign < 0)
		{
			Assert.That(result, Is.LessThan(0));
		}
		else
		{
			Assert.That(result, Is.EqualTo(0));
		}
	}

	[Test]
	public void CompareVersionStrings_StableBeatsPreRelease()
	{
		int result = UnityRegistryClient.CompareVersionStrings("1.0.0", "1.0.0-preview.1");
		Assert.That(result, Is.GreaterThan(0));
	}

	[Test]
	public void CompareVersionStrings_PreReleaseBeforeStable()
	{
		int result = UnityRegistryClient.CompareVersionStrings("1.0.0-preview.1", "1.0.0");
		Assert.That(result, Is.LessThan(0));
	}

	[Test]
	public void CompareVersionStrings_HigherMajor_BeatsLowerMajor()
	{
		int result = UnityRegistryClient.CompareVersionStrings("2.0.0", "1.9.9");
		Assert.That(result, Is.GreaterThan(0));
	}

	#endregion

	#region UnityRegistryClient - FindBestVersion

	[Test]
	public void FindBestVersion_SelectsLatestStableCompatible()
	{
		string json = """
		{
			"versions": {
				"1.0.0": { "unity": "2019.4", "dist": { "tarball": "https://example.com/1.0.0.tgz" } },
				"2.0.0": { "unity": "2019.4", "dist": { "tarball": "https://example.com/2.0.0.tgz" } },
				"3.0.0": { "unity": "2022.3", "dist": { "tarball": "https://example.com/3.0.0.tgz" } }
			}
		}
		""";

		using JsonDocument doc = JsonDocument.Parse(json);
		// Unity 2021.3 should skip 3.0.0 (requires 2022.3)
		PackageVersionInfo? result = UnityRegistryClient.FindBestVersion(
			doc.RootElement, UnityVersion.Parse("2021.3"), "test.package");

		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Version, Is.EqualTo("2.0.0"));
		Assert.That(result.TarballUrl, Is.EqualTo("https://example.com/2.0.0.tgz"));
	}

	[Test]
	public void FindBestVersion_PrefersStableOverPreRelease()
	{
		string json = """
		{
			"versions": {
				"1.0.0": { "unity": "2019.4", "dist": { "tarball": "https://example.com/1.0.0.tgz" } },
				"2.0.0-preview.1": { "unity": "2019.4", "dist": { "tarball": "https://example.com/2.0.0-preview.tgz" } }
			}
		}
		""";

		using JsonDocument doc = JsonDocument.Parse(json);
		PackageVersionInfo? result = UnityRegistryClient.FindBestVersion(
			doc.RootElement, UnityVersion.Parse("2022.3"), "test.package");

		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Version, Is.EqualTo("1.0.0"));
	}

	[Test]
	public void FindBestVersion_FallsBackToPreRelease_WhenNoStable()
	{
		string json = """
		{
			"versions": {
				"1.0.0-preview.1": { "unity": "2019.4", "dist": { "tarball": "https://example.com/preview.tgz" } }
			}
		}
		""";

		using JsonDocument doc = JsonDocument.Parse(json);
		PackageVersionInfo? result = UnityRegistryClient.FindBestVersion(
			doc.RootElement, UnityVersion.Parse("2022.3"), "test.package");

		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Version, Is.EqualTo("1.0.0-preview.1"));
	}

	[Test]
	public void FindBestVersion_ReturnsNull_WhenNoVersions()
	{
		string json = """{ "versions": {} }""";

		using JsonDocument doc = JsonDocument.Parse(json);
		PackageVersionInfo? result = UnityRegistryClient.FindBestVersion(
			doc.RootElement, UnityVersion.Parse("2022.3"), "test.package");

		Assert.That(result, Is.Null);
	}

	[Test]
	public void FindBestVersion_ReturnsNull_WhenNoCompatibleVersions()
	{
		string json = """
		{
			"versions": {
				"1.0.0": { "unity": "2023.1", "dist": { "tarball": "https://example.com/1.0.0.tgz" } }
			}
		}
		""";

		using JsonDocument doc = JsonDocument.Parse(json);
		PackageVersionInfo? result = UnityRegistryClient.FindBestVersion(
			doc.RootElement, UnityVersion.Parse("2019.4"), "test.package");

		Assert.That(result, Is.Null);
	}

	[Test]
	public void FindBestVersion_TreatsNoUnityField_AsCompatibleWithAny()
	{
		string json = """
		{
			"versions": {
				"1.0.0": { "dist": { "tarball": "https://example.com/1.0.0.tgz" } }
			}
		}
		""";

		using JsonDocument doc = JsonDocument.Parse(json);
		PackageVersionInfo? result = UnityRegistryClient.FindBestVersion(
			doc.RootElement, UnityVersion.Parse("2019.4"), "test.package");

		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Version, Is.EqualTo("1.0.0"));
	}

	#endregion
}
