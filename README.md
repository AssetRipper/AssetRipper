# AssetRipper

AssetRipper is a tool for extracting assets from serialized files (*CAB-*\*, *\*.assets*, *\*.sharedAssets*, etc.) and assets bundles (*\*.unity3d*, *\*.assetbundle*, etc.) and converting them into the native engine format.

Important note: This project is currently in an experimental state. Expect bugs and many changes.

Current supported versions: 1.x to 2019.x (support for later versions is in progress)

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

* *AssetRipperCore*

   Core library. It's designed as an single module without any third party dependencies.
   
* *AssetRipperGUI*

   Basic graphic interface application. It has some extra converters, so additionally it exports:
   * AudioClip .wav export
   * Texture2D .png export (with Sprites)
   * Shader DirectX blob export
   * References to build-in Engine assets
   
* *AssetRipperConsole*

   Sample console application which is designed to test Core library functionality.   
   It's a command line application. Drag and drop resource file(s) or/and folder(s) onto the .exe to retrieve the assets. It will then automaticly try to find resource dependencies, create a 'Ripped' folder and extract all supported assets into the created directory.
   As this is a sample application, I'm might not improve it in any way, nor do I make any guarantees about its functionality.


## Requirements:

If you want to build a solution, you'll need:

 \- .NET Framework 4.7.2 + .NET Core 2.0 SDK

 \- Compiler with C# 7.3 syntax support (Visual Studio 2017)


If you want to run binary files, you need to install:

 \- [.NET Framework 4.7.2](https://support.microsoft.com/en-us/help/4054530/microsoft-net-framework-4-7-2-offline-installer-for-windows)
 
 \- [Microsoft Visual C++ 2015](https://www.microsoft.com/en-us/download/details.aspx?id=53840) Redistributables

 \- [Unity 2017.3.0f3 or greater](https://unity3d.com/get-unity/download/archive) (NOTE: your editor version must be no less than the game's version)
 

## Copyright Issues

Please be aware that distributing the output from this software may be against copyright legislation in your jurisdiction. You are responsible for ensuring that you're not breaking any laws.


## Credits

This began as a fork of [mafaca](https://github.com/mafaca)'s [uTinyRipper](https://github.com/mafaca/UtinyRipper) project licensed under the MIT license.

It has borrowed code from [Perfare](https://github.com/Perfare)'s [AssetStudio](https://github.com/Perfare/AssetStudio) project licensed under the MIT license.