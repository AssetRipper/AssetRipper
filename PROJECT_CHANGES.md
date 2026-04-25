# Project Changes

This file summarizes the custom work in this clone.

Clone:
- `AssetRipper-555b574c`
- Based on AssetRipper `1.3.13`
- See `version.txt`

## Added

- `version.txt`
  - stores the downloaded AssetRipper version and commit
- `Source/AssetRipper.Export.UnityProjects/Scripts/ScriptReferenceRelinkerPostExporter.cs`
  - post-export script GUID / `m_Script` relinker
- `Source/AssetRipper.Import/Structure/Assembly/Serializable/ManagedReferenceHelpers.cs`
  - managed-reference helper logic
- `Source/AssetRipper.Import/Structure/Assembly/Serializable/ManagedReferenceResolver.cs`
  - managed-reference object resolver
- `Source/AssetRipper.Import/Structure/Assembly/Serializable/ManagedReferencesRegistryAsset.cs`
  - managed-reference registry reader

## Changed

- `Source/AssetRipper.Export.UnityProjects/ExportHandler.cs`
  - hooks the script relinker into export
- `Source/AssetRipper.Export.UnityProjects/Project/UnityPatches.cs`
  - restores the missing-script helper patch path
- `Source/AssetRipper.Export.UnityProjects/Shaders/DummyShaderTextExporter.cs`
  - improves fallback shader export for more material settings

- `Source/AssetRipper.Import/AssetCreation/GameAssetFactory.cs`
  - changed MonoBehaviour import fallback behavior
  - preserves more data when script layout reads fail
- `Source/AssetRipper.Import/Structure/Assembly/Serializable/SerializableStructure.cs`
  - changed serialized structure read path
  - supports lossy managed-reference fallback instead of empty components
- `Source/AssetRipper.Import/Structure/Assembly/Serializable/UnloadedStructure.cs`
  - updated lazy reload path to use the same fallback logic
- `Source/AssetRipper.Import/Structure/Assembly/Serializable/SerializableValue.cs`
  - updated managed-reference value handling

- `Source/AssetRipper.SerializationLogic/FieldSerializer.cs`
  - changed `[SerializeReference]` handling
  - stops treating normal Unity object fields like managed references
- `Source/AssetRipper.SerializationLogic/SyntheticSerializableType.cs`
  - updated synthetic type generation for managed-reference cases

## Resulting Fixes

- `VRRig` on `Local Gorilla Player` no longer exports as a completely empty component
- `RequestableOwnershipGuard` and `TransferrableItemSlotTransformOverride` no longer collapse to fully empty components
- script references are relinked after export
- texture array export support stays on the newer upstream path
- fallback shader export preserves more material behavior than the old dummy shader

## Removed Again

These custom decompiler changes were added temporarily, then removed:

- vendored ILSpy project integration
- custom decompiler settings / preservation mode
- custom GUI setting for decompiler compatibility

Current state:
- decompiler is back on the normal packaged dependency path
- package version is `AssetRipper.ICSharpCode.Decompiler 10.0.0.8284-preview3`

## Deleted In Final State

- none of the final custom files above were deleted
- the temporary decompiler-only files were removed before finalizing this state

## Notes

- This summary is for the custom work done in this clone.
- It is not a full upstream changelog for everything that differs between AssetRipper `1.3.10` and `1.3.13`.
