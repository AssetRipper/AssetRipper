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

  --log-file       (Default: AssetRipperConsole.log) File to log to.

  -q, --quit      (Default: false) Close console after export.

  --help          Display this help screen.

  --version       Display version information.

  --config-file   Config file to use.

  value pos. 0    Required. Input files or directory to export.
```

## Configuration File

> [!WARNING]
> This is intended for advanced users only.

You may create a configuration file to configure the CLI, similar to how you
would set configuration in the GUI. The configuration file is a simple JSON
file with the following format:

```json
{
  "DefaultVersion": "2021.3.9f1",
  "SpriteExportMode": "Yaml",
  "TextureExportMode": "Png",
}
```

Options available in the configuration file are documented in the source code.
See [LibraryConfiguration.cs](../AssetRipper.Export.UnityProjects/Configuration/LibraryConfiguration.cs)
