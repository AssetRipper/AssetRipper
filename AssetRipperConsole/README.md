# AssetRipperConsole

This is a cross-platform command line application for extracting game assets.

## Usage

Drag and drop resource file(s) or/and folder(s) onto the executable to retrieve the assets. 

Alternatively, use the command prompt / terminal to execute it. For example, on linux:
```
./AssetRipperConsole yourBundle.unity3d
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

## Platform Limitations

Although this is a cross-platform application, some exporters (such as audio, images, and decompiled shaders) are windows-only. On other platforms, these assets will be outputted in the native Unity format.
