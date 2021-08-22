# AssetRipper

![](Images/AssetRipperLogoBackground.png)

[![](https://img.shields.io/github/downloads/ds5678/AssetRipper/total.svg)](https://github.com/ds5678/AssetRipper/releases)
[![](https://img.shields.io/github/downloads/ds5678/AssetRipper/latest/total.svg)](https://github.com/ds5678/AssetRipper/releases/latest)
[![](https://img.shields.io/github/v/release/ds5678/AssetRipper)](https://github.com/ds5678/AssetRipper/releases/latest)

AssetRipper is a tool for extracting assets from serialized files (*CAB-*\*, *\*.assets*, *\*.sharedAssets*, etc.) and assets bundles (*\*.unity3d*, *\*.bundle*, etc.) and converting them into the native Unity engine format.

> Important note: This project is currently in an experimental state. Expect bugs and many changes.

Current supported versions: `3.4.0` to `2021.1.x`


## Donations

Thank you for considering to support me. I have normal expenses like food, electric, internet, and rent. Your donations help to ensure that I can continue to afford developing this project. Anyone with a positive lifetime contribution is entitled to the Donator role on the [Discord server](https://discord.gg/XqXa53W2Yh).

[Patreon](https://www.patreon.com/ds5678)

## Downloads

Milestone download links can be found on the [latest release page](https://github.com/ds5678/AssetRipper/releases/latest).


## Alpha Builds

For advanced users, every commit is automatically built into an alpha build with Github Actions. You can obtain these builds on the [actions page](https://github.com/ds5678/AssetRipper/actions) if you're logged in with a github account.


## Discord [![](https://img.shields.io/discord/867514400701153281?color=blue&label=AssetRipper)](https://discord.gg/XqXa53W2Yh)

The development of this project has a dedicated [Discord server](https://discord.gg/XqXa53W2Yh). Feel free to come say hi. This is also an alternative location for people to post issues.


## Requirements:

If you want to build a solution, you'll need:

 * [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0)
 * [.NET 6](https://dotnet.microsoft.com/download/dotnet/6.0)
 * Compiler with C# 10 syntax support, such as [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)

If you want to run binary files, you need to install:

 * [Unity 2017.3.0f3 or greater](https://unity3d.com/get-unity/download/archive) (NOTE: your editor version must be no less than the game's version)
 

## To Do
 * Predetermined GUID support
 * Extend Windows-only features to Mac and Linux
 * OBJ mesh export
 * GUI settings page
 * NAudio implementation for exporting other audio formats


## Goals
 * Better shader implementation
 * Overhaul struct reading for universal version support
 * Option to reference assemblies instead of scripts


## License

AssetRipper is licensed under the GNU General Public License v3.0


## Copyright Issues

Please be aware that distributing the output from this software may be against copyright legislation in your jurisdiction. You are responsible for ensuring that you're not breaking any laws.


## Credits

This began as a fork of [mafaca](https://github.com/mafaca)'s [uTinyRipper](https://github.com/mafaca/UtinyRipper) project licensed under the [MIT license](Licenses/uTinyRipper.md).

It has borrowed code from [Perfare](https://github.com/Perfare)'s [AssetStudio](https://github.com/Perfare/AssetStudio) project licensed under the [MIT license](Licenses/AssetStudio.md).

It has borrowed code from [spacehamster](https://github.com/spacehamster)'s [uTinyRipperExporter](https://github.com/spacehamster/UtinyRipperExporter) project licensed under the [MIT license](Licenses/uTinyRipperExporter.md).

[Brotli](https://github.com/google/brotli) is licensed under the [MIT license](Licenses/Brotli.md).

[CommandLineParser](https://github.com/commandlineparser/commandline) is licensed under the [MIT license](Licenses/CommandLineParser.md).

[Cpp2IL](https://github.com/SamboyCoding/Cpp2IL) is licensed under the [MIT license](Licenses/Cpp2IL.md).

FMOD is licensed under a [non-commercial license](Licenses/FMOD.md).

[Fmod5Sharp](https://github.com/SamboyCoding/Fmod5Sharp) is licensed under the [MIT license](Licenses/Fmod5Sharp.md).

[HLSLcc](https://github.com/Unity-Technologies/HLSLcc) is licensed under the [MIT license](Licenses/HLSLcc.md).

LibOgg and LibVorbis are licensed from [Xiph](https://www.xiph.org/) under the [BSD 3-Clause License](Licenses/Xiph.md).

[Lz4](https://github.com/lz4/lz4) is licensed under the [MIT license and the BSD 2-Clause license](Licenses/Lz4.md).

[Mono.Cecil](https://github.com/jbevain/cecil) is licensed under the [MIT license](Licenses/MonoCecil.md).

[SharpZipLib](https://github.com/icsharpcode/SharpZipLib) is licensed under the [MIT license](Licenses/SharpZipLib.md).

[Smolv](https://github.com/aras-p/smol-v) is licensed under the [MIT license](Licenses/Smolv.md).

[SpirV](https://github.com/Anteru/csspv) is licensed under the [BSD 2-Clause license](Licenses/SpirV.md).

[texgenpack](https://github.com/hglm/texgenpack) is licensed under a [permissive license](Licenses/texgenpack.md).


## Disclaimer

This software is not sponsored by or affiliated with Unity Technologies or its affiliates. "Unity" is a registered trademark of Unity Technologies or its affiliates in the U.S. and elsewhere.
