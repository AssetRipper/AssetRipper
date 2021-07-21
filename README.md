# AssetRipper

![](Images/AssetRipperLogoBackground.png)

![](https://img.shields.io/github/downloads/ds5678/AssetRipper/total.svg) ![](https://img.shields.io/github/downloads/ds5678/AssetRipper/latest/total.svg)

AssetRipper is a tool for extracting assets from serialized files (*CAB-*\*, *\*.assets*, *\*.sharedAssets*, etc.) and assets bundles (*\*.unity3d*, *\*.assetbundle*, etc.) and converting them into the native Unity engine format.

> Important note: This project is currently in an experimental state. Expect bugs and many changes.

Current supported versions: `1.x` to `2019.x` (support for later versions is in progress)

## Export features
* Scenes
* Prefabs (GameObjects with transform components)
* AnimationClips (legacy, generic, humanoid)
* Meshes
* Shaders (native listing)
* Textures
* Audio
* Fonts
* Movie textures
* Materials
* AnimatorControllers
* Avatars
* Terrains
* TextAssets
* Components:
  * MeshRenderer
  * SkinnedMeshRenderer
  * Animation
  * Animator
  * Canvas
  * Light
  * ParticleSystem
  * Colliders
  * Rigidbody
  * AudioSource
  * Camera
  * MonoBehaviour (Mono only)
  * MonoScript (Mono only)

## Structure

* [*AssetRipperCore*](AssetRipperCore/README.md) (Cross-Platform)

   Core library. It's designed as an single module without any third party dependencies.
   
* [*AssetRipperLibrary*](AssetRipperLibrary/README.md) (Cross-Platform)

   This is an expansion library for AssetRipperCore. It includes some third party dependencies and has some extra converters, so on Windows it additionally exports:
   * AudioClip .wav export
   * Texture2D .png export (with Sprites)
   * References to build-in Engine assets
   * Shader DirectX blob export

* [*AssetRipperGUI*](AssetRipperGUI/README.md) (Windows-only)

   Basic graphic interface application utilizing the AssetRipperLibrary.
   
* [*AssetRipperConsole*](AssetRipperConsole/README.md) (Cross-Platform)

   Command line equivalent of AssetRipperGUI. Since it has no GUI, it can be cross-platform compatible.


## Requirements:

If you want to build a solution, you'll need:

 * [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0)

 * Compiler with C# 9 syntax support, such as [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)


If you want to run binary files, you need to install:

 * [Unity 2017.3.0f3 or greater](https://unity3d.com/get-unity/download/archive) (NOTE: your editor version must be no less than the game's version)
 

## Discord

The development of this project has a dedicated [Discord server](https://discord.gg/XqXa53W2Yh). Feel free to come say hi. This is also an alternative location for people without Github accounts to post issues.


## To Do
 * Update Mono.Cecil
 * Set up proper build actions
 * Better Issue Templates
 * Add some basic unit testing
 * IL2Cpp support


## Goals
 * Unity 2020 and 2021 support
 * Dummy shader implementation
 * NAudio implementation for exporting other audio formats


## License

AssetRipper is licensed under the GNU General Public License v3.0


## Copyright Issues

Please be aware that distributing the output from this software may be against copyright legislation in your jurisdiction. You are responsible for ensuring that you're not breaking any laws.


## Credits

This began as a fork of [mafaca](https://github.com/mafaca)'s [uTinyRipper](https://github.com/mafaca/UtinyRipper) project licensed under the MIT license.

It has borrowed code from [Perfare](https://github.com/Perfare)'s [AssetStudio](https://github.com/Perfare/AssetStudio) project licensed under the MIT license.

[Brotli](https://github.com/google/brotli) is licensed under the MIT license.

[Lz4](https://github.com/lz4/lz4) is licensed under the MIT license and the BSD 2-Clause license.

[Mono.Cecil](https://github.com/jbevain/cecil) is licensed under the MIT license.

[SpirV](https://github.com/Anteru/csspv) is licensed under the BSD 2-Clause license.

[Smolv](https://github.com/aras-p/smol-v) is licensed under the MIT license.

[Cpp2IL](https://github.com/SamboyCoding/Cpp2IL) is licensed under the MIT license.


## Disclaimer

This software is not sponsored by or affiliated with Unity Technologies or its affiliates. "Unity" is a registered trademark of Unity Technologies or its affiliates in the U.S. and elsewhere.