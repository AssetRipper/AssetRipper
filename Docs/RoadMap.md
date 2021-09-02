# Development Roadmap

## 0.1.6.0
 * Extend Windows-only features to Mac and Linux
     * Texture exporter (done!)
     * Audio exporter (partially done)
 * Full script decompilation (done!)
 * OBJ mesh export

## 0.2.0.0
 * Overhaul struct reading, which would enable:
   * universal version support
   * support for exporting any unity component
   * a cleaner, smaller codebase
   * exporting to the original unity version

## Elusive But High Priority
 * Predetermined GUID support

## Unscheduled
 * Option to reference assemblies instead of scripts
 * FBX export
 * Move Third Party Dependencies to Nuget Packages
     * Spirv
     * Smolv
     * Brotli
 * GUI improvements
   * Settings page
   * Search feature
   * Performance enhancements for viewing asset types with large contents, such as 9000 game objects
   * Mesh preview
   * Different background color
   * Export Selected object to project
   * Export Selected object to file