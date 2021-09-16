# Development Roadmap

## 0.1.7.0
* Audio Exporter
  * Build Native Binaries for Mac and Linux
  * Allow other audio formats when not vorbis encoded
* Handle exceptions while reading IL2Cpp assemblies
* Primitive GLTF mesh export
* Merge mesh converter classes into their respective exporters
* Reformat the shader export system to support two sided shaders
* Remove Reading namespace
* Remove remaining Layout classes
* Implement package for LZ4
* GUI improvements
  * Configuration window for choosing import and export options
  * Switch shader preview to use the library shader exporter
  * Better Audio Playback control
    * Fast-forward and rewind
    * Seek-bar for selecting playback position
* Move Third Party Dependencies to Nuget Packages
  * Spirv
  * Smolv
  * Brotli

## 0.2.0.0
* Overhaul struct reading, which would enable:
  * universal version support
  * support for exporting any unity component
  * a cleaner, smaller codebase
  * exporting to the original unity version

## Elusive But High Priority
* Predetermined GUID support

## Planned But Unscheduled
* Option to reference assemblies instead of scripts
* Mesh export options
  * FBX export
  * GLTF export
* Asset Previews
  * Mesh preview
  * Material preview
* Selective Export
  * Export Selected object to folder
  * Export Selected object to compressed zip file
* GUI improvements
  * Performance enhancements for viewing asset types with large contents, such as 9000 game objects
  * Hex Viewer
  * General Settings window
  * Localized text for the UI
  * Adjustable background color/theme
* Import Settings
  * Ignore StreamingAssets directory option
* Export Settings
  * Replace all shaders on materials with a built-in shader (for example, the Standard shader)
  * Script Export
    * IL2Cpp method body reconstruction
    * Stubbing/stripping options

## Concept Ideas
> Note: This is just a collection of ideas. These might not be desirable or feasible, so many of them might never be implemented. Do not interpret their inclusion here as any form of commitment.

* GUI quality of life features
  * Font setting
  * Configurable keybindings
  * Window for licensed works
* Console
  * In the GUI, have a separate window
  * Enterable commands
* Find all references
* Search Window
  * Dedicated window
  * Filters for various asset types
  * Filters for files
  * Might be redundant due to tree view search
* Tree View Search
  * Filters for various asset types
  * Name filter
  * Rows limit
  * Result count
  * Option to group resources
* Tabs
  * Inspector Tab
    * Tag
    * Layer
    * ID
    * File name
    * Asset Specific Properties
      * Sortable
      * Selectable with indepth description below
  * Moveable Tabs
  * Error Tab
* Import Settings
  * Ignore scenes option
  * Import bundle as level
  * Assembly de-obfuscation
* Performance Improvements
  * Asynchronous import/export
* Filtered Export
  * Resources (png, wav, avi, obj, ...)
  * Prefabs
  * Scripts
* Asset Bundle Construction
  * Editing and Repacking of games and asset bundles
  * Asset Header Editor
  * Asset Duplication and Creation
  * Asset Editing and Replacement
  * Copy and paste assets
  * Prefab Creation
* Asset Exporting
  * Video Export (avi, mp4, etc)
  * MP3 audio export
  * Allow saving any text information to a text file
    * Alternatively, ensure that all text can be selected and copied
  * Export as Prefab
  * Export as Unitypackage
  * Node Dump
* Asset Previews
  * Meshes
    * Colored semi-transparency
    * Triangle count
    * FPS meter
    * Corner cube for orientation
    * XYZ axis for orientation
  * Textures
    * Background options (for example: checkers, white, black, default)
    * Disable alpha channel option
  * Font Assets
  * Video Display
  * Animations
  * Scripts
    * view as text
    * notify if script missing