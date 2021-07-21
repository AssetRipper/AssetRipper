# AssetRipperConsole

This is a cross-platform command line application for extracting game assets.

## Usage

Drag and drop resource file(s) or/and folder(s) onto the executable to retrieve the assets. 

Alternatively, use the command prompt / terminal to execute it. For example, on linux:
```
./AssetRipperConsole yourBundle.unity3d
```

While running, it will automaticly try to find resource dependencies, create a 'Ripped' folder, and extract all supported assets into the created directory.

## Platform Limitations

Although this is a cross-platform application, some exporters (such as audio and images) are windows-only. On other platforms, these assets will be outputted in the native Unity format.