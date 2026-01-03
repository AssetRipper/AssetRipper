# Common Issues

## Why doesn't AssetRipper export Mono Scripts from my bundle?

Except in extremely rare and special cases, there are no mono scripts stored in asset bundles.

Mono scripts are stored in C# assemblies, ie `.dll` files. The contents of these assemblies can be viewed in [ILSpy](https://github.com/icsharpcode/ILSpy).

> Note: C++ assemblies also use the `.dll` extension. If C# code can't be viewed after opening a `.dll` file in ILSpy, then it's not a C# assembly.

In order to export mono scripts with asset bundle assets, the relevant assemblies must be included in the import. You can do this by:

1. Place all the assemblies in a folder.
2. Select that folder and the assetbundle(s) being ripped.
3. Drag those into AssetRipper at the same time.

If it worked, you'll get this line in the log:

```text
Import : Files use the 'Mono' scripting backend.
```

If it didn't work, there will be this line:

```text
Import : Files use the 'Unknown' scripting backend.
```

For IL2Cpp games, use [Cpp2IL](https://github.com/SamboyCoding/Cpp2IL) to generate assemblies. [Il2CppInterop assemblies](https://github.com/BepInEx/Il2CppInterop) used in modding will not work.

## Duplicate Assemblies

This has a signature error in the log file.

```text
Could not add pe assembly to name dictionary!
```

It is almost always caused by having two assemblies with the same name in the Managed folder or any subfolders. Note that the "name" in this case is the assembly name shown in a decompiler not the file name.

## Modified Assemblies

> This can include publicized assemblies and/or Il2CppInterop assemblies.

Modified assemblies almost always cause read errors when used in AssetRipper. Here are some common changes that are known to cause lots of problems:

* Publicizing an assembly changes field deserialization on Monobehaviours.
* Attribute removal can also change field deserialization.
* Modified method bodies can cause decompiler errors.

## Too many open files

```text
System.IO.IOException (00000018): Too many open files
```

This can happen on Linux when the limit is set too low. Try running `ulimit -n 1048576` before running AssetRipper in the same terminal. The number 1048576 is arbitrary.

The above command is a soft relaxation of the limit. For permanent changes, modify `/etc/security/limits.conf`.

See [issue 2061](https://github.com/AssetRipper/AssetRipper/issues/2061) for more information.
