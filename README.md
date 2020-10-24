# uTinyRipper
[![Download uTinyRipper](https://a.fsdn.com/con/app/sf-download-button)](https://sourceforge.net/projects/utinyripper/files)

[![Build status](https://ci.appveyor.com/api/projects/status/yd78hqp83f7vjkwb?svg=true)](https://ci.appveyor.com/project/mafaca/utinyripper)
[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/UtinyRipper/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

uTinyRipper is a tool for extracting assets from serialized files (*CAB-*\*, *\*.assets*, *\*.sharedAssets*, etc.) and assets bundles (*\*.unity3d*, *\*.assetbundle*, etc.) and conveting them into native Engine format.

Important note: work on this project is suspended. Author is going to restart it from scratch, redirect development and focus on other things. So don't waste your time on PRs, propositions or complex issues. Only exception is bug fixes.

Supported versions: 1.x to 2019.x (since development is suspended, 2020.x and greater versions won't be supported)

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

* *uTinyRipperCore*

   Core library. It's designed as an single module without any third party dependencies.
   
* *uTinyRipperGUI*

   Basic graphic interface application. It has some extra converters, so additionally it export:
   * AudioClip .wav export
   * Texture2D .png export (with Sprites)
   * Shader DirectX blob export
   * References to build-in Engine assets
   
* *uTinyRipperConsole* and *uTinyRipperConsoleNETCore*

   Sample console application which is designed to test Core library functionality.   
   It is command line console application. Drag and drop resource file(s) or/and folder(s) onto .exe to retrive assets. It will automaticly try to find resource dependencies, create 'Ripped' folder and extract all supported assets into created directory.
   As it is a sample application so I'm not going to improve it in any way.



### Requirements:

If you want to build a solution, you need:

 \- .NET Framework 4.7.2 + .NET Core 2.0 SDK

 \- Compiler with C# 7.3 syntax support (Visual Studio 2017)


If you want to run binary files, you need to install:

 \- [.NET Framework 4.7.2](https://support.microsoft.com/en-us/help/4054530/microsoft-net-framework-4-7-2-offline-installer-for-windows)
 
 \- [Microsoft Visual C++ 2015](https://www.microsoft.com/en-us/download/details.aspx?id=53840) Redistributables

 \- [Unity 2017.3.0f3 or greater](https://unity3d.com/get-unity/download/archive) (NOTE: editor version must be no less than game version)
 
