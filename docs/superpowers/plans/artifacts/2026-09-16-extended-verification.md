# AssetRipper.Processing.Extended — verification against Arcane Rush

Date: 2026-09-16
Build: Release, branch `feature/processing-extended`
Subject: Arcane Rush (Steam 3039040), Unity 6000.0.62f1, macOS, Il2Cpp.
98 collections, 655,108 assets, 59 Addressables bundles.

## Results

| Feature | Real-game result |
|---|---|
| Asset Path Overrides | `Path overrides: applied 1.` — no warnings |
| User-Defined Package Export | **Not verified.** No mined package JSON exists for this game. |
| Asset Deduplication | `1689 duplicate groups, 3810 redundant copies redirected.` 0 hash failures |
| Prefab Outlining (root-level) | `410 repeated hierarchies, 516 duplicates collapsed, 0 rejected on structure mismatch.` |

Load time stayed at ~25 s throughout, matching the unmodified standard edition, so
none of the added processors materially slowed import.

## Deduplication: measured vs estimated

The pre-implementation baseline used a name-based proxy and estimated ~6,573 redundant
copies as an upper bound. Content hashing found **3,810**, or 58% of that. The gap is
the expected one: same-named assets in different bundles are often genuinely different.
The proxy was an upper bound and behaved like one.

`0 rejected on structure mismatch` in outlining means every pair that matched by
signature also survived the strict lockstep re-verification.

## A bug that only real data exposed

The first outlining run reported **0 repeated hierarchies** while all 37 unit tests
passed. The cause was in our code, not the game: `PrefabProcessor` places every
generated hierarchy into a single shared collection named "Prefab Hierarchies", so the
guard requiring a group to span more than one collection could never be satisfied. The
guard had been copied from deduplication without rechecking whether it still applied.

The synthetic fixtures missed it because they placed hierarchies in separate
collections — something the real processor never does. Fixed by grouping on
`hierarchy.Root.Collection` (where the prefab actually came from), and covered by
`CollapsesIdenticalHierarchiesWhoseRootsComeFromDifferentCollections`, which was
confirmed to fail against the old guard and pass against the new one.

## Still open: the SpriteProcessor assertion

Debug builds still crash loading this game:

```
Assertion failed: "Asset already has a main asset assigned."
  AssetGroup.SetMainAsset()              AssetGroup.cs:19
  SpriteInformationObject.SetMainAsset() SpriteInformationObject.cs:90
  SpriteProcessor.Process()              SpriteProcessor.cs:59
```

Deduplication was expected to incidentally fix this and **did not** — verified by
running a Debug build with deduplication enabled, which still asserted. The reason is
visible in our own code: `AssetDeduplicationProcessor.IsCandidate` deliberately excludes
textures whose `MainAsset` is a `SpriteInformationObject`, which are exactly the
textures involved. This is an independent upstream defect and remains unfixed.

## What was not tested

- Package export resolving assets to package GUIDs (no mined data available).
- That a full project export writes overridden assets to their new paths.
- That outlined prefabs round-trip — instantiating them in Unity Editor to confirm they
  reproduce the original hierarchy. This needs the Unity Editor and was not done.
