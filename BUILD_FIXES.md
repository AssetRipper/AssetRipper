# Build Fix and Change Report

## Build target
- Command: `dotnet publish -c Release -r win-x64`
- Project: `Source/AssetRipper.GUI.Free/AssetRipper.GUI.Free.csproj`
- Result: Success (publish output generated)

## Edited files

### 1) `Source/AssetRipper.Export.UnityProjects/ExportHandler.cs`
- Removed `using AssetRipper.Export.Modules.Shaders;`.
- Removed the `ShaderExportMode.Legacy` conditional block that yielded `new ShaderProcessor()`.
- Why: this legacy shader hook referenced symbols not wired into the current project graph and caused compile failure (`CS0234`).
- Outcome: publish can complete for `win-x64`.

### 2) `Source/AssetRipper.Processing/PrefabOutlining/PrefabOutliningProcessor.cs`
- Improved prefab outlining algorithm to handle repeated variants per cleaned name instead of skipping all when multiple variants exist.
- Added deterministic processing order (name sort + path-id tie break).
- Added stronger filtering:
  - skip candidates with fewer than 2 matches,
  - skip candidates already represented by existing prefab roots,
  - skip candidates without scene-backed objects.
- Added unique output naming for multiple outlined variants (`<Name>_Outlined_<N>` with dedupe suffix if needed).
- Improved source selection for cloning (prefer scene root -> any root -> fallback first candidate).
- Added clearer processing logs and summary counters.
- Why: this makes prefab outlining produce useful prefabs more reliably and avoids one-off/noisy outputs.

### 3) `Source/AssetRipper.Processing/PrefabOutlining/GameObjectNameCleaner.cs`
- Reworked cleaning logic to repeatedly strip trailing clone and numeric-copy suffixes until stable.
- Handles mixed suffix orders such as:
  - `Name (Clone) (12)`
  - `Name (12) (Clone)`
- Added safe fallback to `GameObject` when the cleaned result is empty.
- Why: improves grouping accuracy for prefab outline matching.

### 4) `Source/AssetRipper.Tests/PrefabOutlining/GameObjectNameCleanerTests.cs`
- Added NUnit coverage for clone/copy suffix cleanup and fallback behavior (10 test cases).
- Why: locks down core name normalization behavior used by prefab outlining.

## Validation run
- `dotnet test Source/AssetRipper.Tests/AssetRipper.Tests.csproj -c Release --filter GameObjectNameCleanerTests` -> Passed (10/10)
- `dotnet publish -c Release -r win-x64` -> Success
