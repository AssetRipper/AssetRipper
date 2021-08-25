# Development Roadmap

## 0.1.6.0
 * Extend Windows-only features to Mac and Linux
     * Shader exporter (done!)
     * Texture exporter (partially done)
     * Audio exporter (partially done)
 * OBJ mesh export
 * Better shader implementation (done!)
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

## Unscheduled
 * Option to reference assemblies instead of scripts
 * Full script decompilation
 * FBX export
 * GUI improvements
   * Settings page
   * Audio player
   * Mesh preview
   * List shader properties
   * Different background color
   * Export Selected object to project
   * Export Selected object to file