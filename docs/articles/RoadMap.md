# Development Roadmap

## 1.0.16
This release will focus on fixing bugs and improving the user experience in the GUI.

## 1.0.X
These releases will likely focus on cleaning up the project and preparing to release more premium features.

## Planned But Unscheduled
* Import
  * Script Import
    * Use type trees for assembly reconstruction
* Export
  * Script Export
    * Assembly renaming
  * Miscellaneous Export
    * Copy plugins folder into output
  * Primary Content Extraction
  * Binary Export
    * SerializeFile writing as an alternative to yaml export.
* UI
  * Improved Asset previews
    * Add a Mesh preview
    * Scene preview
    * Add a Material preview
* Remove native dependencies
  * Replace crunch with managed code
  * Implement support for decoding Bc textures with managed code for non-multiples of 4

## Concept Ideas
> Note: This is just a collection of ideas. These might not be desirable or feasible, so many of them might never be implemented. Do not interpret their inclusion here as any form of commitment.

* Import
  * Asset Loading
    * Extract assets and save to disk for lower ram usage
* Export
  * Audio Export
    * WWise audio extraction
  * Shader Export
    * Replace all shaders on materials with a built-in shader (for example, the Standard shader)
  * Selective Export
    * Export Selected object to folder
    * Export Selected object to compressed zip file
* Asset editing
* GUI quality of life features
  * Preferences Window
    * Font setting
    * Configurable keybindings?
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
  * Video Display
  * Animations
  * Scripts
    * view as text
    * notify if script missing