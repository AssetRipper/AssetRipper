# Development Roadmap

## 0.1.6.0
 * Extend Windows-only features to Mac and Linux
     * Texture exporter (partially done)
     * Audio exporter (partially done)
 * OBJ mesh export
 * Move Third Party Dependencies to Nuget Packages
     * Spirv
     * Smolv
     * Brotli
 * GUI improvements
     * List shader properties (#63)
     * Hide Yaml tab when unsupported (#63)

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
 * Full script decompilation
 * FBX export
 * GUI improvements
   * Settings page
   * Search feature
   * Performance enhancements for viewing asset types with large contents, such as 9000 game objects
   * Mesh preview
   * Different background color
   * Export Selected object to project
   * Export Selected object to file