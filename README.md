# UtinyRipper
**Latest build:** [![Build status](https://ci.appveyor.com/api/projects/status/yd78hqp83f7vjkwb?svg=true)](https://ci.appveyor.com/project/mafaca/utinyripper/build/artifacts)
[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/UtinyRipper/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

**Latest build (with converters):** [![Build status](https://ci.appveyor.com/api/projects/status/2aqvnu29q68lm3le?svg=true)](https://ci.appveyor.com/project/mafaca/utinyripperfull)

Utiny Ripper is a tool for extracting assets from serialized files (*CAB-*\*, *\*.assets*, *\*.sharedAssets*, etc.) and assets bundles (*\*.unity3d*, *\*.assetbundle*, etc.) and conveting them into native Engine format.

## Export features
* Prefabs (GameObjects with transform components)
* Meshes
* Materials
* Shaders
* Textures (containers and raw only)
* Audio (raw only)
* Movie textures
* Fonts
* AnimationClips (legacy, generic)
* AnimatorControllers
* Avatars
* Terrains
* TextAssets
* Scenes
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
Solution consist of four projects:
* *UtinyRipperCore*

   Core library. It's designed as an single module without any third party dependencies.
* *UtinyRipper*

   Sample console application which is designed to test Core library functionality.   
   It is command line console application. Drag and drop resource file(s) or/and folder(s) onto .exe to retrive assets. It will automaticly try to find resource dependencies, create 'Ripped' folder and extract all supported assets into created directory.
As it is a sample application so I'm not going to improve it in any way.

* *UtinyRipperNETCore* and *UtinyRipperCoreNETStandard*

   They are copies of previous two but target other platform. Those two projects are cross platform and could be lauched on Windows, Linux and Mac using .NET Core frameword runetime.
   
   

### Requirements:

If you want to build solution you need:

 \- .NET Framework 4.5.0 or .NET Standard 1.3 + .NET Core 1.0 SDK

 \- Compiler with C# 7.2 syntax support (Visual Studio 2017)


If you want only to run binaries of sample project you need to install:

 \- [.NET Framework 4.5.0](https://www.microsoft.com/en-us/download/details.aspx?id=30653) or [.NET Core 2.1](https://www.microsoft.com/net/download/dotnet-core/1.0) Runetime

 \- [Unity 2017.3.0f3](https://unity3d.com/get-unity/download/archive)
