# Development Roadmap

## 0.1.9.7

## 0.2.0.0
* Convert Exporters to the new Interface System
  * Animator Controller exporter
  * Mesh exporters
  * Texture exporter
* Move more classes to the Common project
  * Move ProjectExporter
  * Move GameStructure
* Struct Reading Overhaul
  * Apply interfaces in the generated assemblies
  * Implement a system for downloading the assemblies on demand
* Finishing touches
  * Remove the `Classes` and `Converters` namespaces
  * Remove any additional legacy code
  * Merge the common project back into the core project
 
## 0.2.1.0
This release will be primarily focused on cleaning up and refactoring the project. This release may include:
* Unified mesh export
* WWise audio extraction
* Add support for injecting custom asset types
* Add support for reading alternative file formats

## 0.2.2.0
This release will likely focus on removing the native texture dependency by porting essential code to C#.

Other tentative inclusions in this release:
* Build Ogg and Vorbis Native Binaries for Mac and Linux

## 0.2.3.0
This release will likely focus on improving the user experience in the GUI

* Improved Asset previews
  * Preview decompiled script
  * Add a Mesh preview
  * Add a Material preview
  * Add a Hex Viewer
  * Add a TypeTree Viewer
  * Improve the Audio preview
    * Fast-forward and rewind
    * Seek-bar for selecting playback position
    * Visual wave form display
* Preferences window
  * Adjustable background color/theme
* Window for licensed works

## Planned But Unscheduled
* GUI improvements
  * Performance enhancements for viewing asset types with large contents, such as 9000 game objects
  * UI overhaul to resemble Unity Editor
* Export
  * Model Export
    * FBX (full)
    * GLB (full)
    * PMX
    * DAE (aka Collada)
    * Split combined meshes back into the original set of static meshes
  * Script Export
    * Option to reference assemblies instead of scripts
    * Convert TypeTree to MonoScripts
  * Selective Export
    * Export Selected object to folder
    * Export Selected object to compressed zip file
  * Shader Export
    * Replace all shaders on materials with a built-in shader (for example, the Standard shader)
    * Export shaders as yaml

## Concept Ideas
> Note: This is just a collection of ideas. These might not be desirable or feasible, so many of them might never be implemented. Do not interpret their inclusion here as any form of commitment.

* GUI quality of life features
  * Font setting
  * Configurable keybindings
* Console
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
    * Toggle face cull
    * Wireframe option
  * Textures
    * Background options (for example: checkers, white, black, default)
    * Select the color channels to display
      * For example, disable the alpha channel
      * Or show only the blue channel
  * Font Assets
  * Video Display
  * Animations
  * Scripts
    * view as text
    * notify if script missing