:: Build Solution

dotnet build -c Debug ../


:: Generate Dll

cd ./0Bins/AssetRipper.AssemblyDumper/Debug/

AssetRipper.AssemblyDumper.exe

cd ../../../


:: Decompile Dll

cd ./0Bins/AssetRipper.AssemblyDumper.Recompiler/Debug/

AssetRipper.AssemblyDumper.Recompiler.exe ../../AssetRipper.AssemblyDumper/Debug/AssetRipper.SourceGenerated.dll ./Output/ %1

cd ../../../


:: Recompile into a NuGet package

cd ./0Bins/AssetRipper.AssemblyDumper.Recompiler/Debug/Output/

dotnet build -c Release

cd ../../../../


:: Remove the dependency references

cd ./0Bins/AssetRipper.AssemblyDumper.NuGetFixer/Debug/

AssetRipper.AssemblyDumper.NuGetFixer.exe ../../AssetRipper.AssemblyDumper.Recompiler/Debug/Output/bin/Release/ %1