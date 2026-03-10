# AGENT_PLAN

## Current Objective
Transform AssetRipper into the "Ultimate Universal Restoration Suite" in controlled phases, using fast debug builds for verification after code edits.

## Active Assumptions
- Active phase: `Gorilla Tag Depot Import Follow-Up`
- Working target DLL folder: `G:\Projects\Unity Projects\Fall 2024\AuxiliaryFiles\GameAssemblies`
- Scanner path: `G:\Projects\Unity Projects\Fall 2024\AuxiliaryFiles\deep_scanner.py`
- Fast-build command: `dotnet build -c Debug --no-restore`
- Verified workspace fallback when the root `.slnx` host aborts without surfaced errors: `dotnet build -c Debug --no-restore -m:1 -p:BuildInParallel=false`
- Fastest safe Phase 3 inner-loop build: `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false`
- Target exported project version: `2022.3.62f1`
- Scanner integration directive: re-use `deep_scanner.py` package/version detection logic inside AssetRipper where practical instead of treating scan output as a disconnected external artifact
- Previous external/web research is discarded; only scanner output, local package-cache evidence, and current repo code are authoritative

## Follow-Up Scope
- [x] Re-read the live `deep_scanner.py` rules and compare them to `PackageManifestPostExporter` and `RegistryPackageBridge`
- [x] Broaden manifest package-id acceptance to match the scanner's `com.*` / `io.*` rules where an exact version exists
- [x] Expand `Packages/assetripper_versions.txt` so it includes unresolved third-party family detections, not only exact middleware versions
- [x] Reconcile the txt output with a short official-doc web pass for third-party install channels
- [x] Verify with `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false`

## Export Compile Fix Follow-Up
- [x] Re-read the assembly publicizing and script post-export pipeline against the reported Unity compile errors
- [x] Stop publicizing override methods in a way that changes protected base-member signatures
- [x] Deduplicate emitted `NonSerialized` metadata before decompilation
- [x] Add a post-export C# compile fixer for decompiled scripts:
  - duplicate `[NonSerialized]`
  - explicit-layout structs without `FieldOffset`
  - stale job reflection output
  - obvious `Fusion.LogType` vs `UnityEngine.LogType` ambiguity
- [x] Keep plugin-DLL deletion and source-script relinking behavior intact
- [x] Verify with `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false`
- [x] Open the built AssetRipper app after verification

## Native Binary Fallback Follow-Up
- [x] Re-read the native binary exporter against the reported `LightingData` YAML fallback
- [x] Fix the bundle / collection compatibility bug in the native binary export path
- [x] Verify with `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false`

## Manifest Cleanup Follow-Up
- [x] Re-read the exported test manifest and compare it to the scanner seed
- [x] Re-check the live `deep_scanner.py` manifest rules before editing the bridge
- [x] Do a short official-doc web pass for Unity manifest rules and third-party package install channels
- [x] Remove false-positive package ids from manifest generation
- [x] Tighten package-reference harvesting to path-like package contexts
- [x] Verify with `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false`

## Gorilla Tag Depot Import Follow-Up
- [x] Re-read the live import log against the current dependency and lazy-structure code
- [x] Verify the reported `sharedassets*.assets` files really exist in the Gorilla Tag depot
- [x] Preload primary serialized files so on-disk `sharedassets*.assets` are not skipped behind bundle name collisions
- [x] Verify with `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false`

## Phase Checklist
- [x] Phase 0: Run scanner, parse package DNA, document local package-cache evidence, discard bad external research, stop for review
- [x] Phase 1: Recover premium mesh separation, deduplication, package export logic, and unlock premium UI state
- [x] Phase 2: Generate manifest data and bridge script and shader references to local package-cache GUIDs
- [x] Phase 3: Build naming bridge, de-weave networked code, and publicize target assembly members
- [x] Phase 4: Reconstruct native assets for the active `2022.3.62f1` target and keep Unity 6 lighting repair explicitly gated
- [x] Phase 5: Produce the final Windows x64 standalone build

## Phase 0 Status
- [x] Located the scanner and validated its CLI parameters
- [x] Located a likely target game assemblies folder adjacent to the scanner
- [x] Read the current export architecture entry points relevant to manifest and script export
- [x] Run deep scanner and capture `deep_scan_report.json`
- [x] Parse discovered package versions and middleware inventory
- [x] Record local package-cache GUID resolution constraints for registry packages
- [x] Remove the discarded external/web research from long-term notes
- [x] Update research notes with scanner-driven and local-cache findings only
- [x] Stop for user review

## Phase 1 Status
- [x] Review gate cleared by user
- [x] Re-read current export and processing entry points before edits
- [x] Re-read historical premium feature commits and current equivalents
- [x] Restore static mesh separation in the current `AssetRipper.Processing` architecture
- [x] Wire asset deduplication into the current processing/export architecture
- [x] Confirm current DLL export modes already supersede the historical experimental package export commit well enough for Phase 1
- [x] Unlock premium UI state
- [x] Verify with `dotnet build -c Debug --no-restore`

## Phase 2 Status
- [x] Re-read manifest and script export architecture before edits
- [x] Restore a reliable fast-build path under the local .NET 10 SDK
- [x] Add exported script identity map for post-install relinking
- [x] Add editor relinker patch so deleting exported DLLs and installing originals relinks scenes, prefabs, and scriptable assets
- [x] Keep serialized component data intact by rewriting only `m_Script` references
- [x] Adjust `AssetRipper.GUI.Free` debug build settings so fast-build verification does not depend on rewriting a locked `apphost.exe`
- [x] Pin repo-local MSBuild defaults to single-node/no-reuse so `dotnet build -c Debug --no-restore` is stable in this workspace
- [x] Override solution-level `.slnx` fan-out targets to serialize project dispatch during fast-build verification
- [x] Port the scanner's package/version inference into the manifest pipeline
- [x] Resolve local package-cache GUIDs for registry `MonoScript` assets during export
- [x] Redirect package-backed shader references to registry package assets where resolvable
- [x] Emit an exported `Packages` versions text file alongside `manifest.json`
- [x] Verify with `dotnet build -c Debug --no-restore -m:1 -p:BuildInParallel=false`

## Phase 0 Deliverables
- Scan report: `deep_scan_report.json`
- Manifest seed: `deep_scan_manifest.json`
- Research log: `RESEARCH_NOTES.md`

## Phase 3 Status
- [x] Re-read assembly repair and script export architecture before edits
- [x] Add a type-tree naming bridge processor for MonoBehaviour field recovery
- [x] Add a narrow network-property de-weaver for Fusion/Mirror style wrappers
- [x] Force `Assembly-CSharp*` member publicizing without touching package/Discord code
- [x] Verify with `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false`
- [x] Restore the homepage external link buttons for both free and premium UI states
- [x] Verify the edited GUI code with `dotnet build Source/AssetRipper.GUI.Web/AssetRipper.GUI.Web.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false`

## Phase 4 Status
- [x] Re-read native asset, scriptable object, and post-export architecture before edits
- [x] Export Addressables settings, groups, schemas, and templates into standard `Assets/AddressableAssetsData` subfolders with deterministic GUIDs
- [x] Add an editor bootstrap patch that restores `AddressableAssetSettingsDefaultObject.Settings` after the Addressables package is installed
- [x] Force `Texture3D` to fall back to the YAML/native importer path, matching the existing `Texture2DArray` handling
- [x] Add a version-gated binary `.asset` exporter for `LightingData.asset` and `NavMeshData.asset` on modern Unity versions
- [x] Keep Unity 6 renderer/lightmap repair work behind an explicit version gate because the active target is not Unity 6
- [x] Verify with `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false`

## Review Gate
- Release publish build requested out of sequence while the active target remains `2022.3.62f1`; Unity 6 lighting repair stays gated off for this target.
- Important note for later phases: the active target project is `2022.3.62f1`, so Unity 6 lightmap repair logic is compatibility work and must stay gated to Unity 6+ only.
- Previous external/web research was discarded and must not be reused as an implementation source.
- Build reliability note: in this workspace the plain root `dotnet build -c Debug --no-restore` command still intermittently aborts under the local `.slnx` / .NET 10 host with `Build FAILED` and no surfaced errors; the explicit serial no-restore variant above is the verified green path.
- Current Phase 3 inner-loop verification succeeded on the project-scoped build path above.
- Current GUI app folder may be locked by a running `AssetRipper.GUI.Free` process, which can block copying rebuilt `AssetRipper.GUI.Web.dll` into `Source\0Bins\AssetRipper.GUI.Free\Debug`.

## Release Publish Status
- [x] Clean `Build_x64`
- [x] Publish the refreshed release to `Build_x64_refresh` because the old `Build_x64\AssetRipper.GUI.Free.exe` was locked by a running process
- [x] Run `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./Build_x64_refresh`
- [x] Verify the refreshed published executable exists
- [x] Confirm the refreshed output folder contains `Build_x64_refresh\AssetRipper.GUI.Free.exe`
- [x] Publish the final self-contained win-x64 single-file build to `Build_x64_done`
- [x] Verify `Build_x64_done\AssetRipper.GUI.Free.exe --help`
- [x] Verify `Build_x64_done\AssetRipper.GUI.Free.exe --version`

## Release Publish Result
- Publish completed successfully to `G:\New folder\My AssetRipper\AssetRipper\Build_x64`
- Main executable: `G:\New folder\My AssetRipper\AssetRipper\Build_x64\AssetRipper.GUI.Free.exe`
- This release build was produced out of sequence while Phase 4 remains open
- Publish emitted warnings only; no publish errors blocked output generation
- Refreshed release after the Phase 4 changes completed successfully to `G:\New folder\My AssetRipper\AssetRipper\Build_x64_refresh`
- Refreshed main executable: `G:\New folder\My AssetRipper\AssetRipper\Build_x64_refresh\AssetRipper.GUI.Free.exe`
- `Build_x64` could not be overwritten because `Build_x64\AssetRipper.GUI.Free.exe` was locked by a running `AssetRipper.GUI.Free` process
- Final release completed successfully to `G:\New folder\My AssetRipper\AssetRipper\Build_x64_done`
- Final main executable: `G:\New folder\My AssetRipper\AssetRipper\Build_x64_done\AssetRipper.GUI.Free.exe`
- Final verification:
  - `AssetRipper.GUI.Free.exe --help` returned CLI help
  - `AssetRipper.GUI.Free.exe --version` returned `AssetRipper.GUI.Web 1.3.11+693a6cba33c7dd15972ee0a905548dd72a5bd081`
