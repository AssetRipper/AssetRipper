#!/bin/bash

# exit when any command fails
set -e

path_to_OutputFolder="./Output"
path_to_JsonFolder="./TypeTreeDumps/InfoJson"
path_to_AssemblyDumper="./AssemblyDumper.ConsoleApp/bin/x64/ReleaseLinux/net6.0/AssemblyDumper.ConsoleApp"
path_to_RuntimeLibrary="./Libraries/System.Runtime.dll"
path_to_CollectionsLibrary="./Libraries/System.Collections.dll"

mkdir $path_to_OutputFolder

generate() {
    j=$1
    echo Generating from $j...
    $path_to_AssemblyDumper --output $path_to_OutputFolder --runtime $path_to_RuntimeLibrary --collections $path_to_CollectionsLibrary $path_to_JsonFolder/$j
}

if [ $# = 0 ]
then
    vers=($(./get_names.sh))
else
    vers=($(./get_names.sh $1))
fi

echo Generating assemblies for ${#vers[@]} Unity versions
for ((i=0; i<${#vers[@]}; i++)); 
do
    generate ${vers[i]}
done