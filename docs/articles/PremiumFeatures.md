# Premium Features

## Traditional Il2Cpp Recovery

This is an experimental feature for recovering code compiled with Il2Cpp. It takes a traditional decompilation approach. It can be enabled by selecting Script Content Level 3 in the settings.

For performance, mscorlib and any assemblies whose names start with System or Unity have been excluded from analysis. The source code for those assemblies should be publicly available, in one form or another.

From a quick glance, the success rate seems to be currently between 10 and 20 percent of methods on x86 games. Other platforms have lower rates of recovery. Improvements are ongoing.

## Static Mesh Separation

Objects marked as static in a scene get merged when the game is compiled. This is an optimization Unity uses to reduce draw calls. Unfortunately, it also makes game recovery more difficult. This feature reverses that process to the best of its ability.

* If a mesh is used statically multiple times in the game, even across different scenes, AssetRipper intelligently identifies these duplicates and generates a single mesh for all the instances.
* If the original mesh exists in the game files, it's used instead of generating a new mesh.
* Mesh names are lost during static batching, so the GameObject name is used instead. Some sensible name cleaning is applied.

This feature has a setting for enabling it, which defaults to true.

## Prefab Outlining

When a game is compiled, all prefabs in a scene are inlined (instantiated), so any information about the original prefab is lost. This feature attempts to reverse that process by analyzing all GameObject hierarchies in the game and identifying repetitions that can be replaced with new (or existing) prefabs.

This feature has a setting for enabling it, which defaults to false.

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

One of the most frustrating things for users is package export. Previously, exported projects referenced only the default set of Unity core modules. Whenever a user would reference a package that the game had used, the new package would conflict with assemblies and scripts within the exported project. Deleting the conflicting exported files would break any asset references to those files.

There are third-party tools available to help fix broken script references, but they mostly rely on guessing the scripts from the MonoBehaviour fields. In addition, they do nothing to fix broken references for other asset types. That is the purpose of this feature: export asset and package references, so that the user doesn't have to fix broken references later.

Unfortunately, it is not feasible to datamine all possible packages, not even restricted to the official offerings from Unity. As such, users and communities be responsible for mining the packages specific to their game. However, there are some resources to help programmers do the required datamining. https://github.com/AssetRipper/MarrowMiningDemo

This experimental feature can be enabled by going to the Configuration Files page, which can be accessed with "View/Configuration Files" in the file menu. To enable the feature, the user must upload package data json files appropriate for the game being ripped. Example json files are available in the Marrow Mining Demo I linked above.

## Asset Path Overrides

This feature allows users to change the export destination of an asset. It can be enabled by going to the Configuration Files page, which can be accessed with "View/Configuration Files" in the file menu. To enable the feature, the user must upload a json file appropriate for the game being ripped.

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
