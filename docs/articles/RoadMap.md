# Development Roadmap

## 0.3.0.4
This release will be primarily focused on refactoring the project and fixing any reported bugs.

* Refactor the project structure to remove AssetRipper.Core and AssetRipper.Library

## 0.3.X.0
These releases will likely focus on preparing for the next major milestone.

* Remove native dependencies
  * Replace crunch with managed code
  * Implement support for decoding Bc textures with managed code for non-multiples of 4
* Make all dependencies trimmable
* Nuget feed for forked dependencies
* NativeAOT compilation for better performance while loading and extracting
* Use Span readers and writers for more efficient asset reading and writing
* Add donation information to the user interface

## 0.4.0.0 / 1.0.0.0
This release might focus on improving the user experience in the GUI.

## Planned But Unscheduled
* Import
  * Script Import
    * Use type trees for assembly reconstruction
  * Asset Loading
    * Extract assets and save to disk for lower ram usage
* Export
  * Script Export
    * Hybrid script export
    * Assembly renaming
  * Audio Export
    * WWise audio extraction
  * Shader Export
    * Replace all shaders on materials with a built-in shader (for example, the Standard shader)
  * Miscellaneous Export
    * Copy plugins folder into output
  * Selective Export
    * Export Selected object to folder
    * Export Selected object to compressed zip file
  * Primary Content Extraction
  * Binary Export
    * SerializeFile writing as an alternative to yaml export.
* UI
  * Improved Asset previews
    * Preview decompiled scripts
    * Add a Mesh preview
    * Scene preview
    * Font preview
    * Add a Material preview
    * Add a TypeTree Viewer
    * Improve the Audio preview
      * Fast-forward and rewind
      * Seek-bar for selecting playback position
      * Visual wave form display
    * Generic asset preview
      * Add a Hex Viewer
  * Preferences window
  * Window for licensed works
* Asset editing

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