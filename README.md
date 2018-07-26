# UtinyRipper
Latest build: [![Build status](https://ci.appveyor.com/api/projects/status/yd78hqp83f7vjkwb?svg=true)](https://ci.appveyor.com/project/mafaca/utinyripper/branch/master)
[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/UtinyRipper/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Utiny Ripper is a tool for extracting assets from serialized files (CAB-*, *.assets, *.sharedAssets, etc.) and assets bundles (*.unity3d, *.assetbundle, etc.) into native Engine format.

## Export features
* Prefabs (GameObjects with transform components)
* Meshes
* Materials
* Shaders
* Textures (containers and raw only)
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

## Structure
Solution consist of two project:
* UtinyRipperCore 

   Core library. It's designed as an single module without any third party dependencies.
* UtinyRipper

   Test console application. You can use it as an example for your own project.
This is command line console application. Drag and drop any resource file(s) to it to retrive assets. It will automaticly try to find resource dependencies, create 'Ripped' directory and extract all supported asset files into created folder.
This is a sample application and it won't get improved in any way.

### Requirements:

If you want to build solution you need:

.NET Framework 4.7.1 compiler with C# 7.2 syntax support (Visual Studio 2017)

If you want to only run binaries of sample project you need to install:

[.NET Framework 4.6.1](https://www.microsoft.com/en-us/download/details.aspx?id=49982)

[Unity 2017.3.0f3](https://unity3d.com/get-unity/download/archive)
