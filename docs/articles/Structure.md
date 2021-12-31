## Structure

* [*AssetAnalyzer*](https://github.com/AssetRipper/AssetRipper/blob/master/AssetAnalyzer/README.md)

   Simple program to investigate file header information.

* [*AssetRipperCore*](https://github.com/AssetRipper/AssetRipper/blob/master/AssetRipperCore/README.md)

   Core library. It's designed as an single module without any native dependencies.
   
* [*AssetRipperLibrary*](https://github.com/AssetRipper/AssetRipper/blob/master/AssetRipperLibrary/README.md)

   This is an expansion library for AssetRipperCore. It includes some extra exporters:
   * AudioClip export
   * Texture2D export (with Sprites)
   * Dummy Shader Export
   * Text Asset Export
   * Font Export
   * Movie Texture Export
   * Mesh Export
   * Script Export (Mono and IL2Cpp)

* [*AssetRipperGUI*](https://github.com/AssetRipper/AssetRipper/blob/master/AssetRipperGUI/README.md)

   Basic cross-platform graphical interface application utilizing the AssetRipperLibrary.
   
* [*AssetRipperConsole*](https://github.com/AssetRipper/AssetRipper/blob/master/AssetRipperConsole/README.md)

   Command line equivalent of AssetRipperGUI.
   
* [*UnitTester*](https://github.com/AssetRipper/AssetRipper/blob/master/UnitTester/README.md)

   Automated tester to verify project integrity while making changes.