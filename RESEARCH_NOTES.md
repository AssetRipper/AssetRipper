# RESEARCH_NOTES

## Baseline
- Previous external/web research was wrong and is discarded.
- Authoritative inputs, in order:
  - `G:\Projects\Unity Projects\Fall 2024\AuxiliaryFiles\deep_scanner.py`
  - local managed assemblies in `G:\Projects\Unity Projects\Fall 2024\AuxiliaryFiles\GameAssemblies`
  - local Unity package cache `.meta` files
  - current AssetRipper source
- Working DLL folder: `G:\Projects\Unity Projects\Fall 2024\AuxiliaryFiles\GameAssemblies`
- Scanner path: `G:\Projects\Unity Projects\Fall 2024\AuxiliaryFiles\deep_scanner.py`
- Exported project version: `2022.3.62f1`
- Scan outputs:
  - `G:\New folder\My AssetRipper\AssetRipper\deep_scan_report.json`
  - `G:\New folder\My AssetRipper\AssetRipper\deep_scan_manifest.json`
  - `G:\New folder\My AssetRipper\AssetRipper\deep_scan_versions.txt`

## Manifest Follow-Up Intake
- The stale scanner path in older notes is wrong; the readable `deep_scanner.py` copy used for comparison in this follow-up is currently at:
  - `G:\$RECYCLE.BIN\S-1-5-21-3435019205-3387090500-3213848251-1001\$RASZAIM\deep_scanner.py`
- Scanner rules re-confirmed for this pass:
  - exact manifest package ids are recognized with `(?:com|io)\.[...]`
  - family-only detections are still reported even when no exact version is exposed
  - third-party exact versions and unresolved family detections are intentionally separated in the human-readable txt output
- Web verification to use in this pass:
  - Photon Fusion official docs describe package import / setup, not a stable Unity registry package id to hardcode into `manifest.json`
  - mod.io official docs still describe plugin/manual import plus explicit dependency installation rather than a stable Unity registry package id to synthesize
  - Oculus Integration official docs still point to Asset Store / package import flows instead of a stable registry package id to synthesize

## Manifest Follow-Up Result
- `Source/AssetRipper.Export.UnityProjects/Project/PackageManifestPostExporter.cs`
  - no longer hard-rejects non-Unity package ids when they match the scanner-style `(?:com|io)\.[...]` pattern
  - exported text scanning now recognizes exact `com.*` / `io.*` package ids instead of only `com.unity.*`
- `Source/AssetRipper.Export.UnityProjects/Project/RegistryPackageBridge.cs`
  - now records package-family hints from package references as well as explicit `package@version` tags
  - `BuildVersionsReport()` now mirrors the scanner more closely:
    - `Official registry / UPM packages`
    - `Third-party exact versions`
    - `Detected families without exact version`
    - `Package cache resolutions`
- Practical manifest rule after the web pass:
  - if the scanner/export bridge exposes a real `com.*` or `io.*` package id plus exact version, it is eligible for `manifest.json`
  - if a third-party dependency only has a family match or is documented by the vendor as a manual/plugin import flow, it stays in `Packages/assetripper_versions.txt` only
- Relink behavior is unchanged in this pass:
  - deleting exported plugin DLLs and installing source scripts/packages still relies on the existing editor-side `m_Script` relinker bridge
- Verification:
  - `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false` succeeded

## Export Compile Fix Intake
- User-supplied exported-project failures cluster into four buckets:
  - duplicate `[NonSerialized]` attributes
  - protected/internal override signatures made `public`
  - decompiled explicit-layout structs without `FieldOffset`
  - stale/generated source that should not survive export (`__JobReflectionRegistrationOutput__*`, obvious `Fusion.LogType` ambiguity cases)
- Current leverage points re-confirmed from local code:
  - `Source/AssetRipper.Processing/Assemblies/AssemblyCSharpPublicizingProcessor.cs`
    - currently publicizes nearly every method in `Assembly-CSharp*`, including real overrides, which explains the `CS0507` access-mismatch failures
    - currently only checks for the presence of `NonSerialized` before adding one, so pre-existing duplicate metadata is not cleaned up
  - `Source/AssetRipper.Processing/Assemblies/SafeAssemblyPublicizingProcessor.cs`
    - has the same two structural risks when global publicizing is enabled
  - `Source/AssetRipper.Export.UnityProjects/ExportHandler.cs`
    - is the correct place to add a post-export generated-script sanitizer because it runs after script decompilation and before the user opens the Unity project
  - `Source/AssetRipper.Export.UnityProjects/Scripts/ScriptRelinkPostExporter.cs`
    - currently matches installed scripts by `assembly + namespace + class`
    - this is too strict when users delete plugin DLLs and install source scripts whose assembly name changes while namespace/class stay stable
- Planned local-only fix shape:
  - preserve original access on override methods during publicizing
  - deduplicate `NonSerialized` attributes in metadata before ILSpy runs
  - add a post-export text sanitizer over generated `Assets/Scripts/**/*.cs`
  - broaden relink matching with namespace/class and class-only unique fallbacks so deleting DLLs and installing source packages still relinks scenes and prefabs

## Export Compile Fix Result
- `Source/AssetRipper.Processing/Assemblies/PublicizingSupport.cs`
  - centralizes the publicizing safety rules now used by both assembly publicizers
  - preserves inherited override accessibility by skipping true override methods (`virtual + reuse-slot + not new-slot`)
  - removes duplicate `System.NonSerializedAttribute` metadata before decompilation
- `Source/AssetRipper.Processing/Assemblies/AssemblyCSharpPublicizingProcessor.cs`
  - now uses the shared helper so `Assembly-CSharp*` overrides keep their original protected/internal signatures
  - deduplicates `NonSerialized` attributes on all fields before deciding whether to add one
- `Source/AssetRipper.Processing/Assemblies/SafeAssemblyPublicizingProcessor.cs`
  - now uses the same override-preservation and duplicate-attribute cleanup when global publicizing is enabled
- `Source/AssetRipper.Export.UnityProjects/Scripts/ScriptCompileFixPostExporter.cs`
  - added as a version-gated (`2019.1+`) post-export pass over generated `Assets/Scripts/**/*.cs`
  - collapses duplicate `[NonSerialized]` lines in emitted source
  - deletes stale `__JobReflectionRegistrationOutput__*.cs` files before Unity compiles them
  - rewrites obvious `Fusion` / `UnityEngine` `LogType` ambiguities to `UnityEngine.LogType`
  - downgrades broken `StructLayout(LayoutKind.Explicit)` blocks with missing `FieldOffset` coverage to compile-safe sequential layout and strips stale `FieldOffset` attributes in that block
- `Source/AssetRipper.Export.UnityProjects/Scripts/ScriptRelinkPostExporter.cs`
  - relinker still prefers exact `assembly + namespace + class`
  - now falls back to unique `namespace + class`, then unique `class`, so deleting plugin DLLs and installing source scripts/packages can still relink scenes, prefabs, and ScriptableObjects when the assembly name changes
- `Source/AssetRipper.Export.UnityProjects/ExportHandler.cs`
  - now runs the script compile fixer after DLL/script export on `2019.1+` projects
- Verification:
  - `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false` succeeded after the follow-up changes
  - `dotnet build Source/AssetRipper.GUI.Free/AssetRipper.GUI.Free.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false` succeeded after relinking/compile-fix integration
  - the GUI was launched from `Source\0Bins\AssetRipper.GUI.Free\Debug\AssetRipper.GUI.Free.exe`

## Native Binary Fallback Intake
- User test project:
  - `G:\Projects\Unity Projects\Fall 2024\ExportedProject`
- The reported log line matches the catch path in `Source/AssetRipper.Export.UnityProjects/Project/NativeBinaryAssetExporter.cs`.
- Local inspection of the current export output confirms the fallback happened:
  - `G:\Projects\Unity Projects\Fall 2024\ExportedProject\Assets\GorillaTag\Shared\Scenes\Metropolis\LightingData.asset` starts with `%YAML 1.1`
- Root cause from current source:
  - `NativeBinaryExportBuilder.Build()` creates a `ProcessedBundle`
  - then creates nested `BinaryProxyCollection : VirtualAssetCollection` instances inside that bundle
  - `ProcessedBundle` is `VirtualBundle<ProcessedAssetCollection>`, so it rejects `BinaryProxyCollection`
  - that throws `ArgumentException: The collection is not compatible with this Bundle. (Parameter 'collection')`
- Conclusion:
  - the current native binary path is not succeeding for these `LightingDataAsset` exports
  - the YAML output is the fallback path, not the intended primary path

## Native Binary Fallback Result
- `Source/AssetRipper.Export.UnityProjects/Project/NativeBinaryAssetExporter.cs`
  - the temporary export bundle no longer uses `ProcessedBundle`
  - the native writer now uses a dedicated `BinaryProxyBundle : VirtualBundle<BinaryProxyCollection>`
  - this matches the actual collection type created by the binary export path and removes the bundle/collection compatibility exception
- Practical meaning for the user log:
  - the previously observed `ArgumentException: The collection is not compatible with this Bundle. (Parameter 'collection')` was a real exporter bug
  - that exact fallback cause is now fixed in the exporter source
- Verification:
  - `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false` succeeded after the fix
- Remaining limitation:
  - I did not rerun a full export of `G:\Projects\Unity Projects\Fall 2024\ExportedProject` in this pass, so the existing files in that project remain the old YAML-fallback output until you export again with the rebuilt tool

## Manifest Cleanup Intake
- Exported test manifest under `G:\Projects\Unity Projects\Fall 2024\ExportedProject\Packages\manifest.json` currently contains:
  - the expected Unity package set from the scanner seed
  - the default `com.unity.modules.*` entries from `PackageManifest.CreateDefault`
  - one clear false positive: `io.compression.filesystem = 2.1.0.0`
- `io.compression.filesystem` is not present in `deep_scan_manifest.json`, so it is bridge noise rather than scanner-aligned output.
- Current root cause in source:
  - `RegistryPackageBridge.PackageReferenceRegex` matches bare package-like substrings anywhere in extracted assembly strings
  - `RegistryPackageBridge.ManifestDependencies` currently exposes every versioned manifest candidate, including weak `custom_attribute` guesses
- Desired cleanup:
  - keep scanner-style explicit package tags flowing into `manifest.json`
  - stop promoting weak package-like strings from assembly metadata or namespaces into real manifest dependencies
  - keep third-party detections in the txt report, not in `manifest.json`, unless they expose a real package id through reliable package-path / explicit-tag evidence

## Manifest Cleanup Web Pass
- Unity Package Manager manual confirms `Packages/manifest.json` is the dependency source for packages the project actually depends on; this follow-up keeps speculative middleware hints out of that file and leaves them in the txt report instead:
  - https://docs.unity3d.com/Manual/upm-manifestPrj.html
- Photon Fusion official docs still document SDK/package import setup, not a stable Unity registry dependency string to synthesize into `manifest.json`:
  - https://doc.photonengine.com/fusion/current/tutorials/shared-mode-basics/1-getting-started
- mod.io official Unity setup still documents plugin/manual install plus explicit dependency setup rather than a stable registry package id to synthesize:
  - https://docs.mod.io/unity/modio-unity/getting-started
- Meta / Oculus Unity integration documentation still routes through imported SDK/package flows rather than a public Unity registry dependency we can safely hardcode:
  - https://developers.meta.com/horizon/documentation/unity/unity-isdk-getting-started/

## Manifest Cleanup Result
- `Source/AssetRipper.Export.UnityProjects/Project/RegistryPackageBridge.cs`
  - `PackageReferenceRegex` now only harvests package ids from path-like `PackageCache/` or `Packages/` contexts instead of matching any bare `com.*` / `io.*` substring
  - manifest output is now filtered through the same high-signal source shape as the scanner manifest seed:
    - `explicit_tag`
    - `assembly_metadata`
    - `context_strings`
  - weak sources like `custom_attribute` and `assembly_version` still inform the human-readable report when useful, but they no longer flow into `manifest.json`
  - the `Official registry / UPM packages` section in `Packages/assetripper_versions.txt` now mirrors the filtered manifest candidate set instead of dumping every versioned hint
- Practical effect:
  - the false-positive `io.compression.filesystem = 2.1.0.0` path is cut out of exported `manifest.json`
  - third-party/plugin detections continue to belong in `Packages/assetripper_versions.txt` unless the bridge has real package-id evidence
- Verification:
  - `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false` succeeded after the manifest cleanup changes

## Gorilla Tag Depot Import Intake
- Live user import target on March 10, 2026:
  - `D:\Steam\steamapps\content\app_1533390\depot_1533391\Gorilla Tag.exe`
- Import log observations:
  - `sharedassets1.assets`, `sharedassets2.assets`, and `sharedassets6.assets` were reported missing during dependency resolution
  - `Oculus.VR.Editor.dll` / `Oculus.VR.Editor` missing and `Script ID is invalid` for editor-ish assets appeared during lazy MonoBehaviour structure loading
  - processing reached `Editor Format Conversion`; the source log label is exactly `Editor Format Conversion`, so the observed `Conversionfix` suffix is not a real named stage in source
- Local filesystem check against the same depot:
  - `Gorilla Tag_Data` does contain `sharedassets0.assets` through `sharedassets11.assets`
  - therefore the logged `sharedassets1.assets` / `sharedassets2.assets` / `sharedassets6.assets` misses are not expected filesystem-miss noise
- Current code shape:
  - `PlatformGameStructure.CollectDefaultSerializedFiles()` only preloads `globalgamemanagers` / `mainData` and `level*`
  - `sharedassets*.assets`, `resources.assets`, and `globalgamemanagers.assets` are normally left to dependency discovery
  - because `GameBundle.LoadFilesAndDependencies()` deduplicates by serialized file name before late dependency loads, a bundle-internal serialized file name collision can prevent the real on-disk sharedassets file from ever being loaded
- Implementation direction for this follow-up:
  - preload all top-level primary serialized files in the data folder so real player-data dependencies win before any bundle-name collision can suppress them
  - treat the Oculus editor assembly miss and editor-script invalid IDs as secondary noise unless they block export after the sharedassets fix

## Gorilla Tag Depot Import Result
- `Source/AssetRipper.Import/Structure/Platforms/PlatformGameStructure.cs`
  - `CollectDefaultSerializedFiles()` now preloads every top-level primary engine serialized file reported by `IsPrimaryEngineFile(...)`, not just `globalgamemanagers` / `mainData` and `level*`
  - this pulls in on-disk `sharedassets*.assets`, `resources.assets`, and `globalgamemanagers.assets` before late dependency discovery
  - duplicate adds are filtered with an in-method `HashSet<string>`
- Practical effect for the live Gorilla Tag depot:
  - the importer no longer relies on late dependency discovery to notice `sharedassets1.assets`, `sharedassets2.assets`, and `sharedassets6.assets`
  - this should prevent bundle/internal-name collisions from suppressing the real player-data files that already exist in `Gorilla Tag_Data`
- What remains expected/noisy after this change:
  - missing `Oculus.VR.Editor.dll` / `Oculus.VR.Editor` is still consistent with a player build not shipping editor assemblies
  - `Script ID is invalid` on editor-ish or unsupported lazy structures can still appear and is separate from the serialized-file dependency issue
  - the actual source processing stage name is still `Editor Format Conversion`; the earlier observed `Conversionfix` suffix was not a real code path label
- Verification:
  - `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false` succeeded after the import fix

## Local Architecture Read
- `Source/AssetRipper.Export.UnityProjects/Project/PackageManifestPostExporter.cs`
  - Generates `Packages/manifest.json` and now merges scanner-style bridge detections.
- `Source/AssetRipper.Export.UnityProjects/Project/RegistryPackageBridge.cs`
  - Ports the useful package/version inference shape from `deep_scanner.py` into export-time logic.
- `Source/AssetRipper.Export.UnityProjects/Scripts/ScriptExporter.cs`
  - Redirects `MonoScript` pointers to package-cache GUIDs when a local registry package source match exists.
- `Source/AssetRipper.Export.UnityProjects/Shaders/RegistryShaderExporter.cs`
  - Redirects shaders to package-cache GUIDs when a local `.shader.meta` match exists.
- `Source/AssetRipper.Export.UnityProjects/Scripts/ScriptRelinkPostExporter.cs`
  - Emits an editor relinker that rewrites only `m_Script` references after the user installs original scripts/packages.

## Target DNA From Scanner

### UPM Manifest Seed
- `com.autodesk.fbx` -> `4.2.1`
- `com.unity.addressables` -> `1.21.19`
- `com.unity.ai.navigation` -> `1.1.3`
- `com.unity.animation.rigging` -> `1.2.1`
- `com.unity.burst` -> `1.8.7`
- `com.unity.cinemachine` -> `2.9.5`
- `com.unity.collections` -> `2.2.0`
- `com.unity.formats.fbx` -> `4.2.1`
- `com.unity.inputsystem` -> `1.6.1`
- `com.unity.mathematics` -> `1.3.1`
- `com.unity.probuilder` -> `5.0.7`
- `com.unity.profiling.core` -> `1.0.2`
- `com.unity.render-pipelines.core` -> `14.0.8`
- `com.unity.render-pipelines.universal` -> `14.0.8`
- `com.unity.scriptablebuildpipeline` -> `1.21.21`
- `com.unity.shadergraph` -> `14.0.8`
- `com.unity.splines` -> `2.2.1`
- `com.unity.textmeshpro` -> `3.0.9`
- `com.unity.timeline` -> `1.7.4`
- `com.unity.ugui` -> `1.0.0`
- `com.unity.xr.core-utils` -> `2.2.1`
- `com.unity.xr.interaction.toolkit` -> `2.3.2`
- `com.unity.xr.legacyinputhelpers` -> `2.1.10`
- `com.unity.xr.management` -> `4.4.0`
- `com.unity.xr.oculus` -> `3.3.0`

### Third-Party Exact Versions Recovered
- `A* Pathfinding Project` -> `4.2.18`
- `Photon Fusion` -> `2.0.0.807+aff720a2`
- `Photon Realtime` -> `4.1.7.2`
- `Photon PUN 2` -> `2.40`
- `mod.io Unity Plugin` -> `1.4.3.0`
- `Oculus Integration` -> `1.89.0`
- `Voice SDK` -> `57.0.0`
- `Newtonsoft.Json` -> `13.0.2+cf0a246981c33ae00121dfe58b850aefeac1aac0`

### Family-Only / Still Unresolved
- `SteamVR Unity Plugin` -> family known, only `2.2+` generation confidence
- `BakeryRuntimeAssembly.dll` -> family known, exact version not exposed
- `PhotonVoice*.dll` -> family known, exact semver not exposed
- `Meta.WitAi*`, `Meta.Voice*`, `Meta.XR.BuildingBlocks`, `Backtrace`, `PlayFab`, `Oculus.Interaction*`, `Oculus.Platform`, `Oculus.AudioManager`, `Oculus.Spatializer` -> family known, exact package semver not exposed in the scanned managed binaries

## Local Package Cache Evidence
- Cache root: `C:\Users\PC\AppData\Local\Unity\cache\packages\packages.unity.com`
- Exact-version local hits confirmed during Phase 0:
  - `com.unity.textmeshpro@3.0.9`
  - `com.unity.xr.management@4.4.0`
  - `com.unity.xr.legacyinputhelpers@2.1.10`
- Script remap constraint:
  - use the script asset GUID from the package source `.cs.meta`
  - emit `fileID = 11500000`
  - do not maintain a giant hardcoded GUID table
- Shader remap constraint:
  - use the package `.shader.meta` GUID
  - emit the main shader export ID with `AssetType.Meta`
- Cache miss implication:
  - if the exact package version is not present locally, the bridge can only use a nearest-cache fallback or leave the asset on the normal export path

## Phase 2 Implementation Baseline
- The manifest pipeline now merges:
  - embedded `Packages/manifest.json`
  - embedded `Packages/packages-lock.json`
  - exported asset text scanning
  - assembly-name heuristics
  - scanner-style detections from `RegistryPackageBridge`
- Exported projects now write:
  - `Packages/manifest.json`
  - `Packages/assetripper_versions.txt`
- `ScriptExporter` now redirects package-backed `MonoScript` references during export when a package-cache source file match is found.
- `RegistryShaderExporter` now redirects package-backed shader references during export when a package-cache shader match is found.
- `ScriptRelinkPostExporter` remains the fallback bridge when the user deletes exported DLLs and installs original scripts or packages later.

## Phase 3 Intake
- Goal: improve decompiled `Assembly-CSharp*` output for Unity serialization and reduce noisy network wrapper code without touching Discord-specific code.
- Fast inner-loop verification path: `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false`
- Current leverage points:
  - `ObfuscationRepairProcessor` already normalizes property and event names before decompilation.
  - `MonoExplicitPropertyRepairProcessor` already reconstructs missing explicit interface properties.
  - `SafeAssemblyPublicizingProcessor` already knows how to publicize members safely, but it is currently all-or-nothing.
  - `ScriptExportCollection` already rebuilds a recovered type-tree lookup from `IMonoBehaviour.LoadStructure()`.
- Planned Phase 3 direction:
  - move the naming bridge into a processor so field names are fixed before ILSpy runs
  - keep the field rename heuristic conservative and type-tree-driven
  - keep network cleanup scoped to obvious Fusion/Mirror style network property wrappers
  - scope forced publicizing to `Assembly-CSharp*` only

## Phase 3 Implementation Baseline
- `Source/AssetRipper.Processing/Assemblies/TypeTreeNamingBridgeProcessor.cs`
  - builds a recovered field-name map from `IMonoBehaviour.LoadStructure()`
  - matches it against the managed type in the loaded assembly
  - renames fields only when the candidate count lines up cleanly and at least one mismatch looks obfuscated
  - skips injected and Discord-related script identities
- `Source/AssetRipper.Processing/Assemblies/NetworkPropertyDeweavingProcessor.cs`
  - targets `Assembly-CSharp*` only
  - looks for obvious `Fusion` / `Mirror` network-property attributes
  - rewrites getter/setter bodies to a simple backing-field auto-property pattern so ILSpy emits cleaner code
  - skips Discord-related types
- `Source/AssetRipper.Processing/Assemblies/AssemblyCSharpPublicizingProcessor.cs`
  - publicizes only `Assembly-CSharp*` assemblies
  - preserves the `NonSerialized` safeguard for newly public instance fields
  - skips Discord-related types
- `Source/AssetRipper.Export.UnityProjects/ExportHandler.cs`
  - now runs the naming bridge and network de-weaver before script export
  - now always runs the scoped `Assembly-CSharp*` publicizer, while leaving the existing global publicizer behind the user setting
- Verification:
  - `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false` succeeded
  - remaining warnings are the pre-existing `MSB3101` cache-lock warnings and existing `TODO` warnings, not compile failures

## Phase 3 UI Follow-Up
- User requested that the homepage external link buttons remain available even when the UI reports premium.
- Minimal safe interpretation:
  - keep current premium feature gating behavior unchanged
  - do not touch Discord-specific code
  - make the homepage external links visible in both free and premium UI states
- Implementation:
  - `Source/AssetRipper.GUI.Web/Pages/IndexPage.cs` now always renders the existing external link button row
  - premium/free text above the buttons is unchanged
- Verification:
  - `dotnet build Source/AssetRipper.GUI.Web/AssetRipper.GUI.Web.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false` succeeded
  - `dotnet build Source/AssetRipper.GUI.Free/AssetRipper.GUI.Free.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false` can fail when the running GUI locks `Source\0Bins\AssetRipper.GUI.Free\Debug\AssetRipper.GUI.Web.dll`

## Release Publish Intake
- User explicitly requested a release publish build.
- This is being done ahead of the planned phase order; Phase 4 implementation is still open.
- Target output folder: `G:\New folder\My AssetRipper\AssetRipper\Build_x64`

## Release Publish Result
- Command that succeeded:
  - `dotnet publish Source/AssetRipper.GUI.Free/AssetRipper.GUI.Free.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\Build_x64`
- Output verified at:
  - `G:\New folder\My AssetRipper\AssetRipper\Build_x64\AssetRipper.GUI.Free.exe`
- Output folder also contains:
  - appsettings files
  - PDBs for the built projects
  - `capstone.dll`
  - a nested `publish` directory created during the publish pipeline
- Publish warnings observed:
  - existing source warnings in `DiscordStuff.cs` and other project files
  - trim/AOT analysis warnings during the final publish pipeline
  - `ILC` reported that `AssetRipper.Tpk.Compression.LzmaHandler.DecompressStream` will always throw because `SharpCompress.Compressors.LZMA.LzmaStream` constructor resolution was missing
- Outcome:
  - publish completed with exit code `0`
  - release output exists and is shareable
  - the `ILC` missing-method warning should be treated as a runtime-risk note for TPK LZMA decompression in this published configuration
  - after the Phase 4 changes, the original `Build_x64` exe was locked by a running `AssetRipper.GUI.Free` process, so the refreshed release was published to `Build_x64_refresh` instead
  - refreshed command that succeeded:
    - `dotnet publish Source/AssetRipper.GUI.Free/AssetRipper.GUI.Free.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\Build_x64_refresh`
  - refreshed output verified at:
    - `G:\New folder\My AssetRipper\AssetRipper\Build_x64_refresh\AssetRipper.GUI.Free.exe`
  - refreshed executable verification:
    - `AssetRipper.GUI.Free.exe --help` returned CLI help
    - `AssetRipper.GUI.Free.exe --version` returned `AssetRipper.GUI.Web 1.3.11+693a6cba33c7dd15972ee0a905548dd72a5bd081`
  - final command that succeeded:
    - `dotnet publish Source/AssetRipper.GUI.Free/AssetRipper.GUI.Free.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\Build_x64_done`
  - final output verified at:
    - `G:\New folder\My AssetRipper\AssetRipper\Build_x64_done\AssetRipper.GUI.Free.exe`
  - final executable verification:
    - `AssetRipper.GUI.Free.exe --help` returned CLI help
    - `AssetRipper.GUI.Free.exe --version` returned `AssetRipper.GUI.Web 1.3.11+693a6cba33c7dd15972ee0a905548dd72a5bd081`
  - final publish warnings that still matter:
    - existing `DiscordStuff.cs` nullability / unreachable-code warnings remain untouched by request
    - `ILC` still reports that `AssetRipper.Tpk.Compression.LzmaHandler.DecompressStream` will always throw because the `SharpCompress.Compressors.LZMA.LzmaStream` constructor could not be resolved in this published configuration

## Phase 4 Implementation Baseline
- `Source/AssetRipper.Export.UnityProjects/Project/AddressablesLayout.cs`
  - detects Addressables editor ScriptableObjects by script namespace/class
  - routes them into standard `Assets/AddressableAssetsData` subfolders
  - assigns deterministic GUIDs from script identity plus target relative path
- `Source/AssetRipper.Export.UnityProjects/Project/AddressablesScriptableObjectExportCollection.cs`
  - exports those Addressables assets as native YAML `.asset` files with `NativeFormatImporter`
- `Source/AssetRipper.Export.UnityProjects/Project/AddressablesPostExporter.cs`
  - emits an editor bootstrap patch that sets `AddressableAssetSettingsDefaultObject.Settings` to the exported settings asset after the package installs
- `Source/AssetRipper.Export.UnityProjects/Project/ScriptableObjectExporter.cs`
  - now diverts matching Addressables ScriptableObjects into the deterministic layout/export path
- `Source/AssetRipper.Export.UnityProjects/ProjectExporter.Overrides.cs`
  - no longer overrides `Texture3D` to bitmap export, so it falls back to YAML/native importer export like `Texture2DArray`
- `Source/AssetRipper.Export.UnityProjects/ExportHandler.cs`
  - now runs the Addressables post-export bootstrap patch
- `Source/AssetRipper.Export.UnityProjects/Project/NativeBinaryAssetExporter.cs`
  - builds serialized binary `.asset` files for `LightingDataAsset` and `NavMeshData`
  - remaps local and external dependencies through export-aware proxy collections before writing the serialized file
  - stays gated to modern Unity serialized-file generations (`2020+`) and falls back to YAML outside that window
- `Source/AssetRipper.Export.UnityProjects/ProjectExporter.Overrides.cs`
  - now routes `ILightingDataAsset` and `INavMeshData` through the native binary exporter override
- Verification:
  - `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false` succeeded

## Phase 4 Constraints
- The new native binary exporter is only considered safe on modern serialized-file generations, so it is explicitly gated to `2020+`.
- The active target is `2022.3.62f1`, so the Unity 6 GPU Resident Drawer path must stay dormant.
- There is still no locally validated Unity 6 lightmap-offset formula after the research reset; do not claim a Unity 6 correction beyond the explicit version gate.

## Deferred / Unverified
- All previous Unity 6 lightmap research notes were removed.
- Do not implement a Unity 6 correction formula until it is derived from local target evidence or re-verified against the exact engine/package source in use.
- The active target is `2022.3.62f1`, so any Unity 6 work stays gated and deferred for now.

## Phase 1 Historical Recovery Notes
- The commit prefixes in the original phase notes needed correction against the current repository:
  - `AssetEqualityComparer` is `c25b083e6`, not `c25b08366`
  - `2a043e7fb` exists and is the old experimental DLL package export commit
  - `ceea3684f` does not map to static mesh separation in this repository history; the relevant recoverable static mesh processor lives in `Source/AssetRipper.Processing/StaticMeshes/*` before removal at `0ed3fccab`
- Current-tree assessment before edits:
  - `AssetEqualityComparer` already exists in `Source/AssetRipper.Assets/Cloning/AssetEqualityComparer.cs` with a later dedup bugfix than the historical add commit
  - `ScriptExportMode` and `ScriptExportCollection` already contain modern DLL export modes, so the historical package-export commit may already be functionally superseded
  - `EnableStaticMeshSeparation` and `EnableAssetDeduplication` exist in settings/UI, but only static mesh separation code was actually removed from the open repository and dedup wiring is still absent

## Phase 2 Relink Design
- User requirement: when exported DLL scripts are removed and the original package/plugin is installed, scenes, prefabs, and scriptable assets should relink automatically without losing serialized values.
- Direct GUID redirection is sufficient for official registry packages only when exact stable GUIDs are known in advance.
- A more universal bridge is to export a script identity map keyed by the original exported `(guid, fileID)` and resolve the replacement script later in the Unity Editor by `(assembly, namespace, class)`.
- Unity recovery approach selected for this phase:
  - export `Assets/Editor/AssetRipperPatches/ScriptRelinkMap.tsv`
  - export an editor patch that scans installed `MonoScript` assets across `Assets/` and `Packages/`
  - rewrite YAML `m_Script` references in `*.prefab`, `*.unity`, and `*.asset` files from the old DLL/generated pointer to the installed script's `(guid, 11500000)`
- This keeps serialized component payloads intact because only the script pointer line changes.

## Build Reliability Notes
- The local .NET 10 SDK initially failed in `SourceGenerator.Foundations` because `FilterAssembliesTask` was registered through `TaskHostFactory` and the out-of-proc NET x64 host did not come up cleanly.
- Local fix applied in `Source/Directory.Build.targets`:
  - override `FilterAssembliesTask` to use the default in-proc assembly task registration
- After that override and the Phase 1 compile fixes, `dotnet build -c Debug --no-restore -m:1 -p:BuildInParallel=false` succeeded in the workspace.
- The next blocker was `AssetRipper.GUI.Free` failing in `CreateAppHost` with `UnauthorizedAccessException` while rewriting `Source\0Bins\obj\AssetRipper.GUI.Free\Debug\apphost.exe`.
- Local fast-build fix applied in `Source/AssetRipper.GUI.Free/AssetRipper.GUI.Free.csproj`:
  - in `Debug`, set `UseAppHost=false`
  - in `Debug`, set `PublishAot=false`
- This keeps the required debug verification path aligned with the fast-build protocol and avoids touching runtime export logic.
- The remaining instability was the root `dotnet build -c Debug --no-restore` command defaulting to multi-process MSBuild, which in this workspace can abort with `Build FAILED` and no surfaced errors.
- Local build default fix applied in `MSBuild.rsp`:
  - `-m:1`
  - `-p:BuildInParallel=false`
  - `-nr:false`
- Resulting intent: the exact fast-build command stays the same for the user, but the repo now executes it in a deterministic single-node mode.
- Because `MSBuild.rsp` is consumed by MSBuild itself after `dotnet build` has already chosen some defaults, a matching `Directory.Build.rsp` was also added so the .NET CLI forwards the same arguments on the initial command line.
- The generated `AssetRipper.slnx.metaproj` still hardcodes solution fan-out targets with `BuildInParallel="True"`.
- Local solution-level fix applied in `Directory.Solution.targets`:
  - override `Build`, `Clean`, `Rebuild`, and `Publish`
  - reissue the same `MSBuild` task calls with `BuildInParallel="False"`
- This targets the solution wrapper specifically, which is where the plain `dotnet build -c Debug --no-restore` path was still diverging from the successful explicit CLI fallback.
- Current verified state:
  - fastest safe Phase 2 inner-loop build: `dotnet build Source/AssetRipper.Export.UnityProjects/AssetRipper.Export.UnityProjects.csproj -c Debug --no-restore -m:1 -p:BuildInParallel=false`
  - `dotnet build -c Debug --no-restore -m:1 -p:BuildInParallel=false` succeeds
  - the plain root `dotnet build -c Debug --no-restore` command still aborts in this workspace with `Build FAILED` and no surfaced errors under the local `.slnx` / .NET 10 host
  - remaining warnings are file-lock/access warnings on `AssemblyReference.cache` files and `Source\0Bins\AssetRipper.GUI.Free\Debug\AssetRipper.GUI.Free.exe`, but they do not prevent the verified serial build from succeeding
