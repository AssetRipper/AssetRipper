# Development Roadmap

## 1. Planned But Unscheduled

### 1.1 Import
- **Script Import**
  - Use type trees for assembly reconstruction

### 1.2 Export
- **Script Export**
  - Assembly renaming
- **Miscellaneous Export**
  - Copy plugins folder into output
- **Binary Export**
  - SerializeFile writing as an alternative to yaml export

### 1.3 UI
- **Improved Asset Previews**
  - Scene preview
  - Material preview

### 1.4 Remove Native Dependencies
- Replace crunch with managed code

---

## 2. Concept Ideas

> **Note:** This is a collection of ideas that might not be desirable or feasible. Many of them may never be implemented. Do not interpret their inclusion as any form of commitment.

### 2.1 Import/Export Enhancements
- **Asset Loading**
  - Extract assets and save to disk for lower RAM usage
- **Audio Export**
  - WWise audio extraction
- **Shader Export**
  - Replace all shaders on materials with a built-in shader (e.g., Standard shader)
- **Selective Export**
  - Export selected object to folder or compressed zip file
- **Filtered Export**
  - Resources (png, wav, avi, obj, ...)
  - Prefabs
  - Scripts

### 2.2 UI and User Experience
- **GUI Quality of Life Features**
  - Preferences Window (Font setting, Configurable keybindings)
- **Console**
  - Enterable commands
- **Search Functionality**
  - Dedicated search window
  - Tree View Search (Filters, Name filter, Rows limit, Result count, Group resources option)
- **Tabs**
  - Inspector Tab (Tag, Layer, ID, File name, Asset Specific Properties)
  - Moveable Tabs
  - Error Tab

### 2.3 Asset Management
- **Asset Editing**
- **Asset Bundle Construction**
  - Editing and Repacking of games and asset bundles
  - Asset Header Editor
  - Asset Duplication and Creation
  - Asset Editing and Replacement
  - Copy and paste assets
  - Prefab Creation
- **Asset Exporting**
  - Export as Prefab
  - Export as Unitypackage
  - Node Dump

### 2.4 Asset Previews
- **Meshes**
  - Colored semi-transparency
  - Triangle count
  - FPS meter
  - Orientation aids (Corner cube, XYZ axis)
  - Toggle face cull
  - Wireframe option
- **Textures**
  - Background options (e.g., checkers, white, black, default)
  - Color channel display options
- **Other Asset Types**
  - Video Display
  - Animations
  - Scripts (view as text, notify if script missing)

### 2.5 Performance and Settings
- **Import Settings**
  - Ignore scenes option
  - Import bundle as level
  - Assembly de-obfuscation
- **Performance Improvements**
  - Asynchronous import/export

### 2.6 Miscellaneous
- Find all references