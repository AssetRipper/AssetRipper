# AssetRipper

This is a cross-platform gui and command line application for extracting game assets.

## GUI Usage

Double click on the executable to open the graphical user interface.

Alternatively, use the command prompt / terminal to execute it. For example, on linux:
```
./AssetRipper
```

## Command Line Usage

Drag and drop resource file(s) or/and folder(s) onto the executable to retrieve the assets. 

Alternatively, use the command prompt / terminal to execute it. For example, on linux:
```
./AssetRipper yourBundle.unity3d
```

While running, it will automaticly try to find resource dependencies, create a 'Ripped' folder, and extract all supported assets into the created directory.

## Command Line Arguments
```
  -o, --output    Directory to export to. Will be cleared if already exists.

  --logFile       (Default: AssetRipperConsole.log) File to log to.

  -q, --quit      (Default: false) Close console after export.

  --help          Display this help screen.

  --version       Display version information.

  value pos. 0    Required. Input files or directory to export.
```