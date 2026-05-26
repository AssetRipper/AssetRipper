# Premium Features

AssetRipper has several features to make its export more similar to the original project, in order to reduce the difficulty for game developers to compare them.

## Static Mesh Separation

Objects marked as static in a scene get merged when the game is compiled. This is an optimization Unity uses to reduce draw calls. This feature reverses that process to the best of its ability.

* If a mesh is used statically multiple times in the game, even across different scenes, AssetRipper intelligently identifies these duplicates and generates a single mesh for all the instances.
* If the original mesh exists in the game files, it's used instead of generating a new mesh.
* Mesh names are lost during static batching, so the GameObject name is used instead. Some sensible name cleaning is applied.

This feature has a setting for enabling it, which defaults to true.

## Shader Decompilation

This is an experimental shader decompiler that strives to support all variants and preserve perfect semantics. However, it's not yet polished, so expect it to throw errors while ripping some shaders and for there to be compilation errors in Unity Editor. Please report any issues on [GitHub](https://github.com/AssetRipper/AssetRipper/issues).

**Platform Support:**

* Vulkan shaders can be decompiled on any platform.
* DirectX shaders can only be decompiled on Windows computers.

## Prefab Outlining

When a game is compiled, all prefabs in a scene are inlined (instantiated), so any information about the original prefab is lost. This feature attempts to reverse that process by analyzing all GameObject hierarchies in the game and identifying repetitions that can be replaced with new (or existing) prefabs.

This feature has a setting for enabling it, which defaults to false.

## Traditional Il2Cpp Analysis

This is an experimental feature for analyzing code compiled with Il2Cpp. It takes a traditional decompilation approach. It can be enabled by selecting Script Content Level 3 in the settings.

For performance, mscorlib and any assemblies whose names start with System or Unity have been excluded from analysis.

The success rate seems to be currently between 10 and 20 percent of methods on x86 games. Other platforms have lower success rates. Improvements are ongoing.

## Asset Deduplication

When building multiple asset bundles, it's very common for Unity to duplicate assets shared between bundles. This allows each bundle to stay self-contained. Asset deduplication is an experimental feature for reversing that process.

Some assets are easier to deduplicate than others. Currently, this is limited to:

* Mono Scripts
* Shaders
* Compute Shaders
* Audio Clips
* Text Assets
* Meshes
* Textures without sprites

This feature has a setting for enabling it, which defaults to false.

## User Defined Package Export

Without this feature, exported projects reference only the default set of Unity core modules. Whenever a user adds references to packages that their game used, the new packages will conflict with assemblies and scripts within the exported project. Deleting the conflicting exported files will break any asset references to those files.

There are third-party tools available to help fix broken script references, but they mostly rely on guessing the scripts from the MonoBehaviour fields. In addition, they do nothing to fix broken references for other asset types. That is the purpose of this feature: export asset and package references, so that the user doesn't have to fix broken references later.

Unfortunately, it is not feasible to datamine all possible packages, not even restricted to the official offerings from Unity. As such, users are responsible for mining the packages specific to their game. However, there are some resources to help them do the required datamining. https://github.com/AssetRipper/MarrowMiningDemo

This experimental feature can be enabled by going to the Configuration Files page, which can be accessed with "View/Configuration Files" in the file menu. To enable the feature, the user must upload package data json files appropriate for their game. Example json files are available in the Marrow Mining Demo I linked above.

## Asset Path Overrides

This feature allows users to change the export destination of an asset. It can be enabled by going to the Configuration Files page, which can be accessed with "View/Configuration Files" in the file menu. To enable the feature, the user must upload a json file appropriate for their game.

### Path Override File Structure

Path overrides can be supplied as a json file.

```json
{
	"Files": {
		"cab-bcaf22789432bda1e5d0eea9d2521ddd": {
			"4476349470337976665": "Assets/AssetRenamed.txt"
		},
		"level1.assets":
		{
			"1": "Assets/Prefabs/Prefab1.prefab",
			"2": "Assets/Prefabs/Prefab2.prefab",
			"3": "Assets/Prefabs/Subfolder/SpecialPrefab.prefab",
			"12": "Assets/Images/MyTexture.png"
		}
	}
}
```

`Files` is a dictionary with string keys representing asset collection names. The values are also dictionaries. They use the asset path id as the key and the new output path as the value. Paths are relative to the project root.
