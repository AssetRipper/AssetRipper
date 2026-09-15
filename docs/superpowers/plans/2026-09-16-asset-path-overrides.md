# Asset Path Overrides Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let a user redirect the export destination of individual assets by uploading a JSON file, matching the documented premium behaviour.

**Architecture:** A new `AssetRipper.Processing.Extended` project holds an `ExtendedExportHandler : ExportHandler`. Subclassing alone flips `GameFileLoader.Premium` to true, which unlocks the gated UI with no UI edits. The handler registers a `PathOverrides` singleton in `SingletonData`, so the existing `ConfigurationFilesPage` renders the upload slot for free, and splices a `PathOverrideProcessor` into the processor sequence right after `OriginalPathProcessor`.

**Tech Stack:** C# / .NET 10, NUnit 4, System.Text.Json with source-generated contexts (AOT).

**Spec:** `docs/superpowers/specs/2026-09-16-processing-extended-design.md`

## Global Constraints

- Target framework `net10.0`; `Nullable` enabled; `IsAotCompatible=true` — inherited from `Source/Directory.Build.props`. Do not override these in new csproj files.
- `PublishAot=true` in the shipping app. Every JSON type MUST use a source-generated `JsonSerializerContext`. Reflection-based `JsonSerializer` overloads are forbidden — they break under trimming.
- The ONLY existing file this plan modifies is `Source/AssetRipper.GUI.Free/Program.cs` (plus `AssetRipper.slnx` to register new projects). Everything else is new files.
- Set `OverridePath`, never `OriginalPath`. `GetBestDirectory()` and `GetBestName()` both prefer `Override*` over `Original*`, so `OverridePath` is what actually redirects an export. (The spec's prose said `OriginalPath`; `OverridePath` is correct and supersedes it.)
- A stale or wrong override entry MUST log a warning and continue. It must never abort an export.
- Build and run OUTSIDE the Bash sandbox. In-sandbox builds of the heavy projects hang and die at ~5:00 with an empty `Build FAILED` and zero errors. Outside, the full chain builds in 14–17 s.

## JSON format (fixed by `docs/articles/PremiumFeatures.md`)

```json
{
	"Files": {
		"cab-bcaf22789432bda1e5d0eea9d2521ddd": {
			"4476349470337976665": "Assets/AssetRenamed.txt"
		},
		"level1.assets": {
			"1": "Assets/Prefabs/Prefab1.prefab",
			"12": "Assets/Images/MyTexture.png"
		}
	}
}
```

Outer keys are `AssetCollection.Name`. Inner keys are `IUnityObjectBase.PathID`. Values are paths relative to the project root.

---

### Task 1: PathOverrideData model and AOT JSON context

**Files:**
- Create: `Source/AssetRipper.Processing.Extended/AssetRipper.Processing.Extended.csproj`
- Create: `Source/AssetRipper.Processing.Extended/PathOverrides/PathOverrideData.cs`
- Create: `Source/AssetRipper.Processing.Extended/PathOverrides/PathOverrideDataContext.cs`
- Create: `Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj`
- Test: `Source/AssetRipper.Processing.Extended.Tests/PathOverrideDataTests.cs`
- Modify: `AssetRipper.slnx` (register both new projects)

**Interfaces:**
- Consumes: nothing.
- Produces: `PathOverrideData` with `public Dictionary<string, Dictionary<long, string>> Files { get; set; }`, and `PathOverrideDataContext.Default.PathOverrideData` of type `JsonTypeInfo<PathOverrideData>`.

`PathOverrideData` needs a public parameterless constructor because `JsonDataInstance<T>` is constrained to `where T : new()`.

- [ ] **Step 1: Create the two csproj files**

`Source/AssetRipper.Processing.Extended/AssetRipper.Processing.Extended.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<OutputPath>..\0Bins\Other\AssetRipper.Processing.Extended\$(Configuration)\</OutputPath>
		<IntermediateOutputPath>..\0Bins\obj\AssetRipper.Processing.Extended\$(Configuration)\</IntermediateOutputPath>
	</PropertyGroup>

	<ItemGroup>
		<ProjectReference Include="..\AssetRipper.Export.UnityProjects\AssetRipper.Export.UnityProjects.csproj" />
	</ItemGroup>

</Project>
```

`Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<IsPackable>false</IsPackable>
		<OutputPath>..\0Bins\Other\AssetRipper.Processing.Extended.Tests\$(Configuration)\</OutputPath>
		<IntermediateOutputPath>..\0Bins\obj\AssetRipper.Processing.Extended.Tests\$(Configuration)\</IntermediateOutputPath>
		<IsAotCompatible>false</IsAotCompatible>
	</PropertyGroup>

	<ItemGroup>
		<PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.9.0" />
		<PackageReference Include="NUnit" Version="4.6.1" />
		<PackageReference Include="NUnit3TestAdapter" Version="6.2.0" />
	</ItemGroup>

	<ItemGroup>
		<ProjectReference Include="..\AssetRipper.Processing.Extended\AssetRipper.Processing.Extended.csproj" />
	</ItemGroup>

</Project>
```

- [ ] **Step 2: Register both projects in `AssetRipper.slnx`**

Add these two lines alongside the other `<Project Path="Source/..." />` entries, keeping the file's alphabetical ordering:

```xml
  <Project Path="Source/AssetRipper.Processing.Extended/AssetRipper.Processing.Extended.csproj" />
  <Project Path="Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj" />
```

- [ ] **Step 3: Write the failing test**

`Source/AssetRipper.Processing.Extended.Tests/PathOverrideDataTests.cs`:

```csharp
using AssetRipper.Processing.Extended.PathOverrides;
using System.Text.Json;

namespace AssetRipper.Processing.Extended.Tests;

public class PathOverrideDataTests
{
	private const string DocumentedJson = """
		{
			"Files": {
				"cab-bcaf22789432bda1e5d0eea9d2521ddd": {
					"4476349470337976665": "Assets/AssetRenamed.txt"
				},
				"level1.assets": {
					"1": "Assets/Prefabs/Prefab1.prefab",
					"12": "Assets/Images/MyTexture.png"
				}
			}
		}
		""";

	[Test]
	public void DeserializesTheDocumentedFormat()
	{
		PathOverrideData data = JsonSerializer.Deserialize(DocumentedJson, PathOverrideDataContext.Default.PathOverrideData)!;

		Assert.That(data.Files, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{
			Assert.That(data.Files["cab-bcaf22789432bda1e5d0eea9d2521ddd"][4476349470337976665L], Is.EqualTo("Assets/AssetRenamed.txt"));
			Assert.That(data.Files["level1.assets"][1L], Is.EqualTo("Assets/Prefabs/Prefab1.prefab"));
			Assert.That(data.Files["level1.assets"][12L], Is.EqualTo("Assets/Images/MyTexture.png"));
		});
	}

	[Test]
	public void RoundTripsThroughSerialization()
	{
		PathOverrideData original = new();
		original.Files.Add("level0", new Dictionary<long, string> { { 7L, "Assets/Seven.asset" } });

		string json = JsonSerializer.Serialize(original, PathOverrideDataContext.Default.PathOverrideData);
		PathOverrideData restored = JsonSerializer.Deserialize(json, PathOverrideDataContext.Default.PathOverrideData)!;

		Assert.That(restored.Files["level0"][7L], Is.EqualTo("Assets/Seven.asset"));
	}

	[Test]
	public void EmptyJsonObjectYieldsNoOverrides()
	{
		PathOverrideData data = JsonSerializer.Deserialize("{}", PathOverrideDataContext.Default.PathOverrideData)!;

		Assert.That(data.Files, Is.Empty);
	}
}
```

- [ ] **Step 4: Run the test to verify it fails**

```bash
dotnet test Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj
```

Expected: compile error — `PathOverrideData` and `PathOverrideDataContext` do not exist yet.

- [ ] **Step 5: Write the model**

`Source/AssetRipper.Processing.Extended/PathOverrides/PathOverrideData.cs`:

```csharp
namespace AssetRipper.Processing.Extended.PathOverrides;

/// <summary>
/// User-supplied export destinations, keyed by collection name and then by path ID.
/// </summary>
public sealed class PathOverrideData
{
	/// <summary>
	/// Outer key: <see cref="AssetRipper.Assets.Collections.AssetCollection.Name"/>.
	/// Inner key: <see cref="AssetRipper.Assets.IUnityObjectBase.PathID"/>.
	/// Value: a path relative to the project root.
	/// </summary>
	public Dictionary<string, Dictionary<long, string>> Files { get; set; } = [];
}
```

- [ ] **Step 6: Write the JSON context**

`Source/AssetRipper.Processing.Extended/PathOverrides/PathOverrideDataContext.cs`:

```csharp
using System.Text.Json.Serialization;

namespace AssetRipper.Processing.Extended.PathOverrides;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PathOverrideData))]
public sealed partial class PathOverrideDataContext : JsonSerializerContext
{
}
```

- [ ] **Step 7: Run the tests to verify they pass**

```bash
dotnet test Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj
```

Expected: 3 passed. If the `long` dictionary keys fail to bind, that is the real signal that System.Text.Json's source generator needs the numeric-key support — fix it there, do not change the documented JSON format.

- [ ] **Step 8: Commit**

```bash
git add Source/AssetRipper.Processing.Extended Source/AssetRipper.Processing.Extended.Tests AssetRipper.slnx
git commit -m "Add PathOverrideData model with AOT-safe JSON context"
```

---

### Task 2: PathOverrideProcessor

**Files:**
- Create: `Source/AssetRipper.Processing.Extended/PathOverrides/PathOverrideProcessor.cs`
- Test: `Source/AssetRipper.Processing.Extended.Tests/PathOverrideProcessorTests.cs`

**Interfaces:**
- Consumes: `PathOverrideData` from Task 1.
- Produces: `public sealed class PathOverrideProcessor : IAssetProcessor` with constructor `PathOverrideProcessor(PathOverrideData data)` and method `void Process(GameData gameData)`.

Relevant API, already verified against the codebase:
- `gameData.GameBundle.FetchAssetCollections()` returns `IEnumerable<AssetCollection>`.
- `AssetCollection.Name` is a `string`.
- `AssetCollection.TryGetAsset(long pathID)` returns `IUnityObjectBase?`.
- `IUnityObjectBase.OverridePath` is a settable `string?`.
- `Logger.Warning(LogCategory.Processing, message)` from `AssetRipper.Import.Logging`.

- [ ] **Step 1: Write the failing test**

`Source/AssetRipper.Processing.Extended.Tests/PathOverrideProcessorTests.cs`:

```csharp
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Primitives;
using AssetRipper.Processing;
using AssetRipper.Processing.Extended.PathOverrides;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing.Extended.Tests;

public class PathOverrideProcessorTests
{
	private static readonly UnityVersion Version = new(2020, 1);

	private static (GameData GameData, ProcessedAssetCollection Collection) CreateGameData(string collectionName)
	{
		GameBundle bundle = new();
		ProcessedAssetCollection collection = bundle.AddNewProcessedCollection(collectionName, Version);
		GameData gameData = new(bundle, Version, null!, null);
		return (gameData, collection);
	}

	[Test]
	public void AppliesOverrideToMatchingAsset()
	{
		(GameData gameData, ProcessedAssetCollection collection) = CreateGameData("level1.assets");
		IGameObject gameObject = collection.CreateGameObject();

		PathOverrideData data = new();
		data.Files.Add("level1.assets", new Dictionary<long, string> { { gameObject.PathID, "Assets/Prefabs/Renamed.prefab" } });

		new PathOverrideProcessor(data).Process(gameData);

		Assert.That(gameObject.OverridePath, Is.EqualTo("Assets/Prefabs/Renamed.prefab"));
	}

	[Test]
	public void LeavesUnlistedAssetsUntouched()
	{
		(GameData gameData, ProcessedAssetCollection collection) = CreateGameData("level1.assets");
		IGameObject listed = collection.CreateGameObject();
		IGameObject unlisted = collection.CreateGameObject();

		PathOverrideData data = new();
		data.Files.Add("level1.assets", new Dictionary<long, string> { { listed.PathID, "Assets/Listed.prefab" } });

		new PathOverrideProcessor(data).Process(gameData);

		Assert.That(unlisted.OverridePath, Is.Null);
	}

	[Test]
	public void UnknownCollectionNameDoesNotThrow()
	{
		(GameData gameData, _) = CreateGameData("level1.assets");

		PathOverrideData data = new();
		data.Files.Add("does-not-exist.assets", new Dictionary<long, string> { { 1L, "Assets/Nope.asset" } });

		Assert.DoesNotThrow(() => new PathOverrideProcessor(data).Process(gameData));
	}

	[Test]
	public void UnknownPathIdDoesNotThrow()
	{
		(GameData gameData, ProcessedAssetCollection collection) = CreateGameData("level1.assets");
		collection.CreateGameObject();

		PathOverrideData data = new();
		data.Files.Add("level1.assets", new Dictionary<long, string> { { 999999L, "Assets/Nope.asset" } });

		Assert.DoesNotThrow(() => new PathOverrideProcessor(data).Process(gameData));
	}

	[Test]
	public void EmptyDataIsANoOp()
	{
		(GameData gameData, ProcessedAssetCollection collection) = CreateGameData("level1.assets");
		IGameObject gameObject = collection.CreateGameObject();

		new PathOverrideProcessor(new PathOverrideData()).Process(gameData);

		Assert.That(gameObject.OverridePath, Is.Null);
	}
}
```

- [ ] **Step 2: Run the test to verify it fails**

```bash
dotnet test Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj --filter PathOverrideProcessorTests
```

Expected: compile error — `PathOverrideProcessor` does not exist.

- [ ] **Step 3: Write the processor**

`Source/AssetRipper.Processing.Extended/PathOverrides/PathOverrideProcessor.cs`:

```csharp
using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Logging;

namespace AssetRipper.Processing.Extended.PathOverrides;

/// <summary>
/// Applies user-supplied export destinations. Runs after <see cref="AssetRipper.Processing.Scenes.OriginalPathProcessor"/>
/// so that overrides win over the paths it assigns.
/// </summary>
public sealed class PathOverrideProcessor : IAssetProcessor
{
	private readonly PathOverrideData data;

	public PathOverrideProcessor(PathOverrideData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		this.data = data;
	}

	public void Process(GameData gameData)
	{
		if (data.Files.Count == 0)
		{
			return;
		}

		Dictionary<string, AssetCollection> collectionsByName = [];
		foreach (AssetCollection collection in gameData.GameBundle.FetchAssetCollections())
		{
			// Collection names are not guaranteed unique. First one wins; warn on the rest.
			if (!collectionsByName.TryAdd(collection.Name, collection))
			{
				Logger.Warning(LogCategory.Processing, $"Path overrides: more than one collection is named '{collection.Name}'. Using the first.");
			}
		}

		int applied = 0;
		foreach ((string collectionName, Dictionary<long, string> overrides) in data.Files)
		{
			if (!collectionsByName.TryGetValue(collectionName, out AssetCollection? collection))
			{
				Logger.Warning(LogCategory.Processing, $"Path overrides: no collection named '{collectionName}'. Skipping its {overrides.Count} entries.");
				continue;
			}

			foreach ((long pathID, string path) in overrides)
			{
				IUnityObjectBase? asset = collection.TryGetAsset(pathID);
				if (asset is null)
				{
					Logger.Warning(LogCategory.Processing, $"Path overrides: '{collectionName}' has no asset with path ID {pathID}. Skipping.");
					continue;
				}

				if (string.IsNullOrEmpty(path))
				{
					Logger.Warning(LogCategory.Processing, $"Path overrides: empty path for {pathID} in '{collectionName}'. Skipping.");
					continue;
				}

				asset.OverridePath = path;
				applied++;
			}
		}

		Logger.Info(LogCategory.Processing, $"Path overrides: applied {applied}.");
	}
}
```

- [ ] **Step 4: Run the tests to verify they pass**

```bash
dotnet test Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj
```

Expected: 8 passed (3 from Task 1, 5 here).

- [ ] **Step 5: Commit**

```bash
git add Source/AssetRipper.Processing.Extended/PathOverrides/PathOverrideProcessor.cs Source/AssetRipper.Processing.Extended.Tests/PathOverrideProcessorTests.cs
git commit -m "Add PathOverrideProcessor applying user-supplied export destinations"
```

---

### Task 3: ExtendedExportHandler and app wiring

**Files:**
- Create: `Source/AssetRipper.Processing.Extended/ExtendedExportHandler.cs`
- Modify: `Source/AssetRipper.GUI.Free/Program.cs`
- Modify: `Source/AssetRipper.GUI.Free/AssetRipper.GUI.Free.csproj` (add the project reference)
- Test: `Source/AssetRipper.Processing.Extended.Tests/ExtendedExportHandlerTests.cs`

**Interfaces:**
- Consumes: `PathOverrideProcessor`, `PathOverrideData`, `PathOverrideDataContext` from Tasks 1–2.
- Produces: `public sealed class ExtendedExportHandler : ExportHandler` with constructor `ExtendedExportHandler(FullConfiguration settings)` and `public const string PathOverridesKey = "PathOverrides"`.

Verified API:
- `ExportHandler` has `protected FullConfiguration Settings { get; }` and `protected virtual IEnumerable<IAssetProcessor> GetProcessors()`.
- `SingletonDataStorage.Add(string key, DataInstance value)` and `TryGetStoredValue<T>(string key, out T value)`.
- `JsonDataInstance<T>(JsonTypeInfo<T>)` requires `T : new()`.
- `GameFileLoader.ExportHandler` has a public setter that calls `ThrowIfSettingsDontMatch`, so it MUST be constructed with `GameFileLoader.Settings`.

- [ ] **Step 1: Write the failing test**

`Source/AssetRipper.Processing.Extended.Tests/ExtendedExportHandlerTests.cs`:

```csharp
using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Processing.Extended.PathOverrides;

namespace AssetRipper.Processing.Extended.Tests;

public class ExtendedExportHandlerTests
{
	[Test]
	public void IsNotTheBaseTypeSoThePremiumUiGatesUnlock()
	{
		// GameFileLoader.Premium is `ExportHandler.GetType() != typeof(ExportHandler)`.
		ExportHandler handler = new ExtendedExportHandler(new FullConfiguration());

		Assert.That(handler.GetType(), Is.Not.EqualTo(typeof(ExportHandler)));
	}

	[Test]
	public void RegistersThePathOverridesConfigurationSlot()
	{
		FullConfiguration settings = new();

		_ = new ExtendedExportHandler(settings);

		Assert.That(settings.SingletonData.Keys, Contains.Item(ExtendedExportHandler.PathOverridesKey));
	}

	[Test]
	public void PathOverridesStartEmpty()
	{
		FullConfiguration settings = new();

		_ = new ExtendedExportHandler(settings);

		Assert.That(settings.SingletonData.TryGetStoredValue(ExtendedExportHandler.PathOverridesKey, out PathOverrideData? data), Is.True);
		Assert.That(data!.Files, Is.Empty);
	}
}
```

- [ ] **Step 2: Run the test to verify it fails**

```bash
dotnet test Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj --filter ExtendedExportHandlerTests
```

Expected: compile error — `ExtendedExportHandler` does not exist.

- [ ] **Step 3: Write the handler**

`Source/AssetRipper.Processing.Extended/ExtendedExportHandler.cs`:

```csharp
using AssetRipper.Configuration;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Processing.Extended.PathOverrides;
using AssetRipper.Processing.Scenes;

namespace AssetRipper.Processing.Extended;

/// <summary>
/// Adds the extended feature set on top of the standard <see cref="ExportHandler"/>.
/// Subclassing also flips <c>GameFileLoader.Premium</c>, which unlocks the gated settings UI.
/// </summary>
public sealed class ExtendedExportHandler : ExportHandler
{
	public const string PathOverridesKey = "PathOverrides";

	public ExtendedExportHandler(FullConfiguration settings) : base(settings)
	{
		settings.SingletonData.Add(PathOverridesKey, new JsonDataInstance<PathOverrideData>(PathOverrideDataContext.Default.PathOverrideData));
	}

	protected override IEnumerable<IAssetProcessor> GetProcessors()
	{
		foreach (IAssetProcessor processor in base.GetProcessors())
		{
			yield return processor;

			// After: overrides must win over the paths OriginalPathProcessor just assigned.
			if (processor is OriginalPathProcessor
				&& Settings.SingletonData.TryGetStoredValue(PathOverridesKey, out PathOverrideData? pathOverrides)
				&& pathOverrides.Files.Count > 0)
			{
				yield return new PathOverrideProcessor(pathOverrides);
			}
		}
	}
}
```

- [ ] **Step 4: Run the tests to verify they pass**

```bash
dotnet test Source/AssetRipper.Processing.Extended.Tests/AssetRipper.Processing.Extended.Tests.csproj
```

Expected: 11 passed.

- [ ] **Step 5: Wire it into the app**

Add the project reference to `Source/AssetRipper.GUI.Free/AssetRipper.GUI.Free.csproj`, inside the existing `<ItemGroup>` that already references `AssetRipper.GUI.Web`:

```xml
		<ProjectReference Include="..\AssetRipper.Processing.Extended\AssetRipper.Processing.Extended.csproj" />
```

Replace the whole of `Source/AssetRipper.GUI.Free/Program.cs` with:

```csharp
using AssetRipper.GUI.Web;
using AssetRipper.Processing.Extended;

GameFileLoader.ExportHandler = new ExtendedExportHandler(GameFileLoader.Settings);

WebApplicationLauncher.Launch(args);
```

- [ ] **Step 6: Build and verify the UI unlocks**

```bash
dotnet build Source/AssetRipper.GUI.Free/AssetRipper.GUI.Free.csproj -c Release
```

Expected: build succeeded. Then run it and confirm the gate flipped:

```bash
dotnet Source/0Bins/AssetRipper.GUI.Free/Release/AssetRipper.GUI.Free.dll --port 17780 --headless
```

In another shell:

```bash
curl -s http://localhost:17780/ConfigurationFiles | grep -c PathOverrides
```

Expected: at least 1 — the upload slot is rendered by the existing page with no UI code written.

Also confirm the previously disabled checkboxes are now enabled:

```bash
curl -s http://localhost:17780/Settings/Edit | grep -c 'EnableAssetDeduplication.*disabled'
```

Expected: `0`.

- [ ] **Step 7: Commit**

```bash
git add Source/AssetRipper.Processing.Extended/ExtendedExportHandler.cs Source/AssetRipper.Processing.Extended.Tests/ExtendedExportHandlerTests.cs Source/AssetRipper.GUI.Free/Program.cs Source/AssetRipper.GUI.Free/AssetRipper.GUI.Free.csproj
git commit -m "Wire ExtendedExportHandler into the app and register path overrides"
```

---

### Task 4: Verify against Arcane Rush

**Files:**
- Create: `docs/superpowers/plans/artifacts/2026-09-16-path-overrides-verification.md` (findings)

**Interfaces:**
- Consumes: the running app from Task 3.
- Produces: a recorded pass/fail against real data.

- [ ] **Step 1: Start the app**

```bash
dotnet Source/0Bins/AssetRipper.GUI.Free/Release/AssetRipper.GUI.Free.dll --port 17781 --headless
```

- [ ] **Step 2: Upload an override file**

Pick a real collection name and path ID from the loaded game first, then POST the JSON. The `Content` form field takes the file text directly:

```bash
curl -s -X POST http://localhost:17781/ConfigurationFiles/Singleton/Add \
  --data-urlencode "Key=PathOverrides" \
  --data-urlencode 'Content={"Files":{"globalgamemanagers.assets":{"1":"Assets/Overridden/Proof.asset"}}}'
```

- [ ] **Step 3: Load the game**

```bash
curl -s -X POST http://localhost:17781/LoadFolder \
  --data-urlencode "Path=/Users/iamsuperman/Library/Application Support/Steam/steamapps/common/Arcane Rush/ArcaneRush.app"
```

Expected: HTTP 302, roughly 25 s.

- [ ] **Step 4: Confirm the override was applied**

Check the log for the summary line the processor emits:

```
Path overrides: applied 1.
```

Expected: `applied 1`, and no warnings about a missing collection or path ID. If it says `applied 0`, the collection name or path ID was wrong — read them off the loaded game rather than guessing.

- [ ] **Step 5: Record the findings and commit**

Write what was overridden, the observed log line, and any surprises into the artifact file, then:

```bash
git add docs/superpowers/plans/artifacts/2026-09-16-path-overrides-verification.md
git commit -m "Record path override verification against Arcane Rush"
```

---

## Self-Review

**Spec coverage.** The spec's §1 Asset Path Overrides is covered by Tasks 1–3; the architecture section (new project, subclass, one-line injection, type-detection splicing) by Tasks 1 and 3; the testing section by the NUnit fixtures in Tasks 1–2 and the real-game run in Task 4. The spec's other three features are deliberately out of this plan — they get their own plans, per the skill's scope rule that each plan must produce working software on its own.

**Deviation from spec, recorded deliberately.** The spec's prose said to set `OriginalPath`. That is wrong: `GetBestDirectory()` and `GetBestName()` prefer `Override*`, so only `OverridePath` actually redirects an export. The Global Constraints section states the correction.

**Known risk carried into Task 1.** `Dictionary<long, string>` relies on System.Text.Json's numeric dictionary-key support under source generation. Step 7 of Task 1 calls this out explicitly so it is caught at the first test run rather than at integration.

**Deferred to the next plan.** The `AssetDeduplicationProcessor` and `PrefabOutliningProcessor` splices shown in the spec's `GetProcessors()` sample are intentionally absent from Task 3's handler — they do not exist yet. The handler gains them when their plans land.
