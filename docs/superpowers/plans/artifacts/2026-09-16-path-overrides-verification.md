# Asset Path Overrides — verification against Arcane Rush

Date: 2026-09-16
Build: Release, `feature/processing-extended`

## Subject

Arcane Rush (Steam 3039040), Unity 6000.0.62f1, macOS, Il2Cpp.
98 collections, 655,108 assets.

## UI gating

Subclassing `ExportHandler` alone flipped `GameFileLoader.Premium`. No UI code was
written, yet:

| Check | Result |
|---|---|
| `PathOverrides` upload slot rendered on `/ConfigurationFiles` | present |
| `EnableAssetDeduplication` checkbox carries `disabled` | no |
| `EnableStaticMeshSeparation` checkbox carries `disabled` | no |

## End-to-end override

Collection names were read off the loaded game rather than guessed. Real names
include `level0`, `level1`, `globalgamemanagers.assets`, `sharedassets0.assets`,
`unity_builtin_extra`, `unity default resources`.

Uploaded:

```json
{"Files":{"level1":{"1":"Assets/Overridden/Proof.prefab"}}}
```

`level1` path ID 1 is a GameObject. After reloading the game:

```
Processing : Path overrides: applied 1.
```

No warnings about a missing collection or path ID.

## Regression

Loading the game through `ExtendedExportHandler` succeeded in 25 s with zero errors,
matching the unmodified standard edition's baseline. The subclass does not disturb
the existing load path.

## Not covered here

The export itself was not run, so this confirms the override reaches the asset, not
that the written file lands at the new path. The `OverridePath` property is what
`GetBestDirectory()` and `GetBestName()` consume, so the remaining risk is low, but
it is untested.
