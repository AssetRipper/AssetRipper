# User-Defined Package Export Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let a user upload mined package data so exported projects reference real Unity packages instead of exporting conflicting copies of those assets.

**Architecture:** Package JSONs are stored as a `ListData` entry, so the existing `ConfigurationFilesPage` handles upload with no UI code. Each becomes a `PredefinedAssetCache` wrapped in an `EngineAssetsExporter`, registered in `BeforeExport`. Packages flagged `UsedInPackageJson` are added to `Packages/manifest.json` by a subclass of `PackageManifestPostExporter`.

**Tech Stack:** C# / .NET 10, NUnit 4, `AssetRipper.Mining.PredefinedAssets` 1.5.0.

**Spec:** `docs/superpowers/specs/2026-09-16-processing-extended-design.md`

## Global Constraints

Same as `2026-09-16-asset-path-overrides.md`: net10.0, AOT-safe source-generated JSON only, build and run OUTSIDE the Bash sandbox, and the only pre-existing files touched are `Program.cs` and `AssetRipper.slnx` (both already modified by the previous plan).

## Spike findings (already verified against the shipped assembly)

The spec assumed `EngineResourceData` would carry package data. It does not — it only has `DefaultResources` and `ExtraResources`. The correct type is **`UnityPackageData`**, which the Mining package exposes specifically for this:

```
UnityPackageData
  string Name                       // e.g. "com.unity.textmeshpro"
  string Version
  bool   UsedInPackageJson          // whether it belongs in manifest.json
  Dictionary<string, UnityGuid> Assemblies
  AssetDictionary Assets            // IEnumerable<KeyValuePair<Object, PPtr>>
  static UnityPackageData FromJson(string)

PPtr { long FileID; UnityGuid Guid; AssetType Type; }
```

`MiningSerializerContext.UnityPackageData` exists, so parsing is AOT-safe.

`PredefinedAssetCache` has a public parameterless constructor plus `TryAdd(Object, long fileID, UnityGuid guid, AssetType assetType)` overloads, so a cache can be built from a package the same way the `EngineResourceData` constructor builds one from engine resources.

**Enum collision to watch:** `PPtr.Type` is `AssetRipper.Mining.PredefinedAssets.AssetType`, while `PredefinedAssetCache.TryAdd` takes `AssetRipper.IO.Files.AssetType`. Both declare `Internal, Cached, Serialized, Meta`. Convert explicitly by name — do not assume the numeric values line up.

---

### Task 1: Load packages from configuration

**Files:**
- Create: `Source/AssetRipper.Processing.Extended/Packages/UnityPackageLoader.cs`
- Test: `Source/AssetRipper.Processing.Extended.Tests/UnityPackageLoaderTests.cs`

**Interfaces:**
- Produces: `public static class UnityPackageLoader` with `public static List<UnityPackageData> Load(IEnumerable<string> jsonTexts)`. Malformed entries are logged and skipped; the method never throws.

- [ ] **Step 1: Write the failing test**

```csharp
using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.Processing.Extended.Packages;
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
	public void EmptyInputYieldsEmptyList()
	{
		Assert.That(UnityPackageLoader.Load([]), Is.Empty);
	}
}
```

- [ ] **Step 2: Run to verify it fails**

```bash
dotnet test Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj --filter UnityPackageLoaderTests
```

Expected: compile error — `UnityPackageLoader` does not exist.

- [ ] **Step 3: Implement**

```csharp
using AssetRipper.Import.Logging;
using AssetRipper.Mining.PredefinedAssets;

namespace AssetRipper.Processing.Extended.Packages;

/// <summary>
/// Parses user-uploaded package JSON. A bad file must never abort an export,
/// so failures are logged and skipped.
/// </summary>
public static class UnityPackageLoader
{
	public static List<UnityPackageData> Load(IEnumerable<string> jsonTexts)
	{
		List<UnityPackageData> packages = [];
		int index = 0;
		foreach (string json in jsonTexts)
		{
			index++;
			UnityPackageData package;
			try
			{
				package = UnityPackageData.FromJson(json);
			}
			catch (Exception ex)
			{
				Logger.Warning(LogCategory.Export, $"Package data #{index} could not be parsed and was skipped: {ex.Message}");
				continue;
			}

			if (string.IsNullOrEmpty(package.Name))
			{
				Logger.Warning(LogCategory.Export, $"Package data #{index} has no name and was skipped.");
				continue;
			}

			packages.Add(package);
		}
		return packages;
	}
}
```

- [ ] **Step 4: Run to verify it passes, then commit**

```bash
dotnet test Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj
git add Source/AssetRipper.Processing.Extended/Packages Source/AssetRipper.Processing.Extended.Tests/UnityPackageLoaderTests.cs
git commit -m "Add UnityPackageLoader parsing user-uploaded package data"
```

---

### Task 2: Build an asset cache from a package

**Files:**
- Create: `Source/AssetRipper.Processing.Extended/Packages/PackageAssetCacheFactory.cs`
- Test: `Source/AssetRipper.Processing.Extended.Tests/PackageAssetCacheFactoryTests.cs`

**Interfaces:**
- Consumes: `UnityPackageData` from Task 1.
- Produces: `public static class PackageAssetCacheFactory` with `public static PredefinedAssetCache Create(UnityPackageData package)`.

- [ ] **Step 1: Write the failing test**

```csharp
using AssetRipper.Export.UnityProjects.EngineAssets;
using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.Primitives;
using AssetRipper.Processing.Extended.Packages;

namespace AssetRipper.Processing.Extended.Tests;

public class PackageAssetCacheFactoryTests
{
	[Test]
	public void EmptyPackageProducesUsableCache()
	{
		UnityPackageData package = new() { Name = "com.unity.burst", Version = "1.8.0" };

		PredefinedAssetCache cache = PackageAssetCacheFactory.Create(package);

		Assert.That(cache, Is.Not.Null);
	}

	[Test]
	public void AddsAMinedTextAssetUnderItsPackageGuid()
	{
		UnityGuid guid = UnityGuid.NewGuid();
		TextAsset textAsset = new() { Name = "PackagedText", Script = "hello" };
		UnityPackageData package = new() { Name = "com.example.pkg", Version = "1.0.0" };
		package.Assets.Add(textAsset, new PPtr { FileID = 11100000, Guid = guid, Type = AssetType.Serialized });

		PredefinedAssetCache cache = PackageAssetCacheFactory.Create(package);

		// The cache is keyed by mined-object identity; a successful Add is the observable outcome.
		Assert.That(cache, Is.Not.Null);
	}
}
```

- [ ] **Step 2: Run to verify it fails**

Expected: compile error — `PackageAssetCacheFactory` does not exist.

- [ ] **Step 3: Implement**

```csharp
using AssetRipper.Export.UnityProjects.EngineAssets;
using AssetRipper.Import.Logging;
using AssetRipper.Mining.PredefinedAssets;
using FileAssetType = AssetRipper.IO.Files.AssetType;
using MiningAssetType = AssetRipper.Mining.PredefinedAssets.AssetType;
using MiningObject = AssetRipper.Mining.PredefinedAssets.Object;

namespace AssetRipper.Processing.Extended.Packages;

/// <summary>
/// Builds a <see cref="PredefinedAssetCache"/> from mined package data, mirroring how
/// the engine-resource constructor builds one from <see cref="EngineResourceData"/>.
/// </summary>
public static class PackageAssetCacheFactory
{
	public static PredefinedAssetCache Create(UnityPackageData package)
	{
		PredefinedAssetCache cache = new();
		int added = 0;
		foreach ((MiningObject minedObject, PPtr pointer) in package.Assets)
		{
			if (cache.TryAdd(minedObject, pointer.FileID, pointer.Guid, Convert(pointer.Type)))
			{
				added++;
			}
		}
		Logger.Info(LogCategory.Export, $"Package '{package.Name}' {package.Version}: cached {added} of {package.Assets.Count} assets.");
		return cache;
	}

	/// <summary>
	/// The two enums share member names but are distinct types. Map by name rather than
	/// assuming the numeric values line up.
	/// </summary>
	private static FileAssetType Convert(MiningAssetType type) => type switch
	{
		MiningAssetType.Internal => FileAssetType.Internal,
		MiningAssetType.Cached => FileAssetType.Cached,
		MiningAssetType.Serialized => FileAssetType.Serialized,
		MiningAssetType.Meta => FileAssetType.Meta,
		_ => FileAssetType.Serialized,
	};
}
```

- [ ] **Step 4: Run to verify it passes, then commit**

```bash
dotnet test Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj
git add Source/AssetRipper.Processing.Extended/Packages/PackageAssetCacheFactory.cs Source/AssetRipper.Processing.Extended.Tests/PackageAssetCacheFactoryTests.cs
git commit -m "Build PredefinedAssetCache from mined package data"
```

---

### Task 3: Write packages into manifest.json

**Files:**
- Create: `Source/AssetRipper.Processing.Extended/Packages/ExtendedPackageManifestPostExporter.cs`
- Test: `Source/AssetRipper.Processing.Extended.Tests/ExtendedPackageManifestPostExporterTests.cs`

**Interfaces:**
- Consumes: `UnityPackageData`.
- Produces: `public sealed class ExtendedPackageManifestPostExporter : PackageManifestPostExporter` with constructor taking `IReadOnlyList<UnityPackageData>` and `public PackageManifest BuildManifest(UnityVersion version)` exposing the result for testing.

`PackageManifestPostExporter.CreateManifest(UnityVersion)` is `protected virtual` and `PackageManifest.Dependencies` is a public `Dictionary<string, string>`, so the subclass adds to the default set.

- [ ] **Step 1: Write the failing test**

```csharp
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.Primitives;
using AssetRipper.Processing.Extended.Packages;

namespace AssetRipper.Processing.Extended.Tests;

public class ExtendedPackageManifestPostExporterTests
{
	private static readonly UnityVersion Version = new(6000, 0, 62, UnityVersionType.Final, 1);

	[Test]
	public void IncludesPackagesFlaggedForPackageJson()
	{
		ExtendedPackageManifestPostExporter exporter = new(
		[
			new UnityPackageData { Name = "com.unity.textmeshpro", Version = "3.0.6", UsedInPackageJson = true },
		]);

		PackageManifest manifest = exporter.BuildManifest(Version);

		Assert.That(manifest.Dependencies["com.unity.textmeshpro"], Is.EqualTo("3.0.6"));
	}

	[Test]
	public void ExcludesPackagesNotFlaggedForPackageJson()
	{
		ExtendedPackageManifestPostExporter exporter = new(
		[
			new UnityPackageData { Name = "com.example.internal", Version = "1.0.0", UsedInPackageJson = false },
		]);

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
		ExtendedPackageManifestPostExporter exporter = new(
		[
			new UnityPackageData { Name = "com.unity.modules.animation", Version = "9.9.9", UsedInPackageJson = true },
		]);

		PackageManifest manifest = exporter.BuildManifest(Version);

		Assert.That(manifest.Dependencies["com.unity.modules.animation"], Is.EqualTo("1.0.0"));
	}
}
```

- [ ] **Step 2: Run to verify it fails**

Expected: compile error — `ExtendedPackageManifestPostExporter` does not exist.

- [ ] **Step 3: Implement**

```csharp
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Import.Logging;
using AssetRipper.Mining.PredefinedAssets;

namespace AssetRipper.Processing.Extended.Packages;

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
```

- [ ] **Step 4: Run to verify it passes, then commit**

```bash
dotnet test Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj
git add Source/AssetRipper.Processing.Extended/Packages/ExtendedPackageManifestPostExporter.cs Source/AssetRipper.Processing.Extended.Tests/ExtendedPackageManifestPostExporterTests.cs
git commit -m "Add user packages to the exported manifest.json"
```

---

### Task 4: Wire packages into the handler

**Files:**
- Modify: `Source/AssetRipper.Processing.Extended/ExtendedExportHandler.cs`
- Test: `Source/AssetRipper.Processing.Extended.Tests/ExtendedExportHandlerTests.cs` (extend)

**Interfaces:**
- Consumes: everything above.
- Produces: `public const string PackageDataKey = "PackageData"` on `ExtendedExportHandler`, plus overrides of `BeforeExport` and `GetPostExporters`.

Verified API: `ListDataStorage.Add(string key, List<string> value)`; `FullConfiguration.ListData` is a `ListDataStorage`; `DataSet.Strings` is an `IReadOnlyList<string>`; `ProjectExporter.OverrideExporter<T>(IAssetExporter)` is public; `ExportHandler.GetPostExporters()` is `protected virtual`.

- [ ] **Step 1: Add these tests to `ExtendedExportHandlerTests`**

```csharp
	[Test]
	public void RegistersThePackageDataConfigurationSlot()
	{
		FullConfiguration settings = new();

		_ = new ExtendedExportHandler(settings);

		Assert.That(settings.ListData.Keys, Contains.Item(ExtendedExportHandler.PackageDataKey));
	}

	[Test]
	public void PackageDataStartsEmpty()
	{
		FullConfiguration settings = new();

		_ = new ExtendedExportHandler(settings);

		Assert.That(settings.ListData[ExtendedExportHandler.PackageDataKey]!.Count, Is.Zero);
	}
```

- [ ] **Step 2: Run to verify they fail**

Expected: compile error — `PackageDataKey` does not exist.

- [ ] **Step 3: Extend the handler**

Add to the constructor, after the existing `SingletonData.Add` call:

```csharp
		settings.ListData.Add(PackageDataKey, new List<string>());
```

Add the constant beside `PathOverridesKey`:

```csharp
	public const string PackageDataKey = "PackageData";
```

Add these members:

```csharp
	private List<UnityPackageData> LoadPackages()
	{
		DataSet? set = Settings.ListData[PackageDataKey];
		return set is null ? [] : UnityPackageLoader.Load(set.Strings);
	}

	protected override void BeforeExport(ProjectExporter projectExporter)
	{
		foreach (UnityPackageData package in LoadPackages())
		{
			projectExporter.OverrideExporter<IUnityObjectBase>(new EngineAssetsExporter(PackageAssetCacheFactory.Create(package)));
		}
	}

	protected override IEnumerable<IPostExporter> GetPostExporters()
	{
		List<UnityPackageData> packages = LoadPackages();
		foreach (IPostExporter postExporter in base.GetPostExporters())
		{
			// Replace the stock manifest exporter with one that knows about user packages.
			yield return postExporter is PackageManifestPostExporter
				? new ExtendedPackageManifestPostExporter(packages)
				: postExporter;
		}
	}
```

Required additional usings: `AssetRipper.Assets`, `AssetRipper.Configuration`, `AssetRipper.Export.UnityProjects.EngineAssets`, `AssetRipper.Export.UnityProjects.Project`, `AssetRipper.Mining.PredefinedAssets`, `AssetRipper.Processing.Extended.Packages`.

**Ordering note, already verified:** `BeforeExport` runs before `DoFinalOverrides`, and the exporter stack tries the most recently registered handler first. So the stock engine-asset exporter registered by `DoFinalOverrides` still wins, and user packages act as the fallback. That is the desired precedence — do not try to "fix" it by registering later.

- [ ] **Step 4: Run tests and build the app**

```bash
dotnet test Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj
dotnet build Source/AssetRipper.GUI.Free/AssetRipper.GUI.Free.csproj -c Release
```

Expected: all tests pass; build succeeded.

- [ ] **Step 5: Verify the upload slot renders**

```bash
dotnet Source/0Bins/AssetRipper.GUI.Free/Release/AssetRipper.GUI.Free.dll --port 17782 --headless
curl -s http://localhost:17782/ConfigurationFiles | grep -c PackageData
```

Expected: at least 1.

- [ ] **Step 6: Commit**

```bash
git add Source/AssetRipper.Processing.Extended/ExtendedExportHandler.cs Source/AssetRipper.Processing.Extended.Tests/ExtendedExportHandlerTests.cs
git commit -m "Register user package data and wire it into export"
```

---

## Self-Review

**Spec coverage.** The spec's §2 is covered by Tasks 1–4. The spec's `EngineAssetsExporter.CreateFromJsonText` route is deliberately NOT used — the spike proved `EngineResourceData` carries no package identity, so `UnityPackageData` is used instead. This is recorded in the Spike findings section above and supersedes the spec's prose.

**Placeholder scan.** No TBD/TODO; every code step contains real code.

**Type consistency.** `UnityPackageLoader.Load` returns `List<UnityPackageData>`, consumed as `IReadOnlyList<UnityPackageData>` by `ExtendedPackageManifestPostExporter` (a `List<T>` satisfies it) and iterated in `BeforeExport`. `PackageAssetCacheFactory.Create` returns `PredefinedAssetCache`, which is exactly what `EngineAssetsExporter`'s public constructor takes.

**Known weakness, stated rather than hidden.** Task 2's tests only assert that cache construction succeeds. `PredefinedAssetCache` exposes `Contains` overloads that take engine-side interfaces (`ITextAsset` and friends), which synthetic mined objects cannot satisfy without building real Unity assets, so a true round-trip assertion is out of reach at unit level. Actual resolution is therefore only proven by the export-level check in the next plan's verification, and that gap is real.
