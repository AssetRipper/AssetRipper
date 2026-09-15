# AssetRipper.Processing.Extended — Design

Date: 2026-09-16

Open-source implementations of four features that the AssetRipper premium edition
provides. Written from the public feature list and `docs/articles/PremiumFeatures.md`;
all code is original.

## Goal

Make the standard edition's export closer to the original Unity project, along the
four axes that are tractable in pure C# inside this repository.

In scope:

1. Asset Path Overrides
2. User-Defined Package Export
3. Asset Deduplication
4. Prefab Outlining (root-level only in v1)

Out of scope, with reasons:

| Feature | Why excluded |
|---|---|
| Static Mesh Separation | The validation game has zero combined meshes, so the heuristic could not be checked against real data. Revisit when a game with static batching is available. |
| Shader Decompilation | Needs a native SPIR-V / DXBC decompiler backend. Separate project. |
| Il2Cpp Analysis | Needs deep Cpp2IL work. Separate project. |

## Baseline

Measured against **Arcane Rush** (Steam 3039040), Unity 6000.0.62f1, macOS, Il2Cpp,
loaded through the unmodified standard edition in 25 s:

- 98 collections, 655,108 assets
- 128,226 GameObjects but only 4,390 PrefabInstances — large outlining headroom
- 219 meshes, **0 combined** — the basis for excluding Static Mesh Separation
- Name-proxy duplicate scan across the 59 Addressables bundles: ~6,573 redundant
  copies, concentrated in MonoScript (3,206) and Texture2D (2,659)

The 6,573 figure is an **upper bound from a name-based proxy**, not content hashing.
Two same-named assets in different collections can legitimately differ. Real
deduplication must hash content; the proxy only establishes that the opportunity is
large enough to be worth building.

### Pre-existing bug found while establishing the baseline

A Debug build crashes loading this game:

```
Assertion failed: "Asset already has a main asset assigned."
  AssetGroup.SetMainAsset()                      AssetGroup.cs:19
  SpriteInformationObject.SetMainAsset()         SpriteInformationObject.cs:90
  SpriteProcessor.Process()                      SpriteProcessor.cs:59
```

One texture is claimed by two `SpriteInformationObject` groups, which is what
duplication across Addressables bundles produces. Release builds compile the
assertion out and load fine. This is an upstream defect, not caused by this design;
deduplication running before `SpriteProcessor` may incidentally fix it. Tracked
separately — it should not be conflated with this work.

## Architecture

One new project, `Source/AssetRipper.Processing.Extended/`, referencing
`AssetRipper.Export.UnityProjects`.

The free/premium split in this codebase runs through a single seam:

```csharp
// GameFileLoader.cs:40
public static bool Premium => ExportHandler.GetType() != typeof(ExportHandler);
```

Subclassing `ExportHandler` therefore flips every premium UI gate on with no UI edits.
`ExportHandler` exposes two `virtual` hooks, both marked in upstream source as premium
extension points: `GetProcessors()` (carrying the literal comment
`//Static mesh separation goes here`) and `BeforeExport(ProjectExporter)`.

```csharp
public sealed class ExtendedExportHandler(FullConfiguration settings) : ExportHandler(settings)
{
    protected override IEnumerable<IAssetProcessor> GetProcessors()
    {
        foreach (IAssetProcessor processor in base.GetProcessors())
        {
            // Before: deduplicate so every later processor sees canonical assets.
            if (processor is EditorFormatProcessor && Settings.ProcessingSettings.EnableAssetDeduplication)
                yield return new AssetDeduplicationProcessor();

            // Before: outline while PrefabProcessor has not yet claimed hierarchies.
            if (processor is PrefabProcessor && Settings.ProcessingSettings.EnablePrefabOutlining)
                yield return new PrefabOutliningProcessor();

            yield return processor;

            // After: overrides must win over the paths just assigned.
            if (processor is OriginalPathProcessor && TryGetPathOverrides(out PathOverrideData overrides))
                yield return new PathOverrideProcessor(overrides);
        }
    }
}
```

Splicing by **type detection** rather than copying the base list means upstream can add
or reorder processors without this fork drifting.

Injection is one line in `Source/AssetRipper.GUI.Free/Program.cs`:

```csharp
GameFileLoader.ExportHandler = new ExtendedExportHandler(GameFileLoader.Settings);
```

That file is the only existing file this design modifies.

### Constraints

- `PublishAot=true`, so every new JSON config type needs a source-generated
  `JsonSerializerContext`. Reflection-based serialization will not survive trimming.
- Settings already exist upstream (`EnablePrefabOutlining`, `EnableAssetDeduplication`)
  but nothing reads them. They become live once the processors above consume them.

## 1. Asset Path Overrides

Schema is already fixed by `docs/articles/PremiumFeatures.md`:

```json
{ "Files": { "<collection name>": { "<pathID>": "Assets/New/Path.ext" } } }
```

`AssetRipper.Configuration` plus `ConfigurationFilesPage` already provide the whole
upload/store/clear UI. Registering a singleton entry in the handler's constructor makes
the upload slot appear with no new UI code:

```csharp
Settings.SingletonData.Add("PathOverrides", new JsonDataInstance<PathOverrideData>(ctx));
```

`PathOverrideProcessor` runs after `OriginalPathProcessor` and, for each asset matching
`(collection name, pathID)`, sets `OriginalPath`. Unmatched entries are logged as
warnings rather than failing the run — a stale override file should not abort an export.

## 2. User-Defined Package Export

The spike resolved the unknown. `AssetRipper.Mining.PredefinedAssets` 1.5.0 is already a
dependency, and the OSS path for engine assets is:

```csharp
EngineAssetsExporter.CreateFromJsonText(json)   // -> EngineResourceData.FromJson
  -> new PredefinedAssetCache(resourceData)     // Contains(...) -> (fileID, guid, assetType)
```

User packages reuse that exact path. Package JSONs are registered as a **ListData**
entry (many files, matching the documented workflow), and each becomes an
`EngineAssetsExporter` registered in `BeforeExport`:

```csharp
protected override void BeforeExport(ProjectExporter projectExporter)
{
    foreach (string json in userPackageJsons)
        projectExporter.OverrideExporter<IUnityObjectBase>(EngineAssetsExporter.CreateFromJsonText(json));
}
```

Ordering works out: `BeforeExport` runs before `DoFinalOverrides`, and the exporter
stack tries the most recently registered handler first. So Unity's own engine assets
still win, and user packages act as the fallback — which is the desired precedence.
`EngineAssetsExporter.TryCreateCollection` returns false for assets outside its cache,
so the stack falls through cleanly.

A `PackageManifestPostExporter` extension writes the referenced packages into
`Packages/manifest.json`.

## 3. Asset Deduplication

Restricted to the seven types the documentation lists: MonoScript, Shader,
ComputeShader, AudioClip, TextAsset, Mesh, and Texture2D without sprites.

Three stages:

1. **Hash** each candidate by content, per type. Not by name — the baseline proxy used
   names only because it was cheap, and it overcounts.
2. **Group** by hash and elect a canonical asset, preferring a collection that is not an
   asset bundle so the survivor lands in a stable path.
3. **Remap** every `PPtr` aimed at a duplicate to the canonical asset, then record the
   losers in the existing `DeletedAssetsInformation`.

Reuses `TraversalHelperMethods` for PPtr walking and the existing script-hashing work
(`ScriptHashingTests`) for MonoScript identity.

Runs before `EditorFormatProcessor` so that every later processor — sprite, prefab,
scriptable object — sees canonical assets rather than duplicates.

## 4. Prefab Outlining (v1: root-level)

Reduce each GameObject subtree to a structural signature: the multiset of component
class IDs plus the ordered signatures of its children, deliberately excluding volatile
data such as world position. Signatures occurring at least twice are outlining
candidates; the **maximal** repeated subtree wins so nested repetitions do not explode
into thousands of tiny prefabs.

One instance becomes the prefab asset; the others become `PrefabInstance`s whose
`m_Modifications` carry their differences.

**v1 is limited to subtrees that are complete GameObject roots.** `PrefabProcessor`
assumes each GameObject belongs to exactly one hierarchy, and outlined prefabs nested
inside a scene violate that assumption. Nested outlining is deferred to v2 rather than
risking corrupt hierarchies in v1.

## Testing

Each feature follows the same loop:

1. NUnit fixtures built in memory, following the existing `ExportTests` /
   `StrippedAssetTests` patterns.
2. A run against Arcane Rush to tune heuristics on real data.

Per-feature success criteria:

| Feature | Criterion |
|---|---|
| Path Overrides | Overridden assets land at the specified paths; stale entries warn, never abort |
| Package Export | Referenced package assets resolve to package GUIDs instead of exported copies |
| Deduplication | Redundant copies drop measurably; **no reference breaks** — every remapped PPtr resolves |
| Prefab Outlining | Outlined prefabs round-trip: instantiating them reproduces the original hierarchy |

Deduplication's criterion is the strict one. A wrong remap silently corrupts the export,
so a full reference-integrity sweep runs after the processor in tests.

## Order of work

Ascending risk, so each step is validated before the next depends on it:

1. Asset Path Overrides — smallest, exercises the config-file plumbing end to end
2. User-Defined Package Export — reuses a proven OSS code path
3. Asset Deduplication — highest measured payoff on the validation game
4. Prefab Outlining — highest risk, and benefits from deduplication running first

## Environment note

Builds and runs must happen outside the Bash sandbox: in-sandbox builds of the heavy
projects hang and die at ~5:00 with an empty `Build FAILED` and zero errors, and the web
server cannot bind a port. Outside it, the full `GUI.Free` chain builds in 14–17 s.
