## Structure

* [*AssetAnalyzer*](../AssetAnalyzer/README.md)

   Simple program to investigate file header information.

* [*AssetRipperCore*](../AssetRipperCore/README.md)

   Core library. It's designed as an single module without any third party dependencies.
   
* [*AssetRipperLibrary*](../AssetRipperLibrary/README.md)

   This is an expansion library for AssetRipperCore. It includes some extra exporters:
   * AudioClip export
   * Texture2D export (with Sprites)
   * Shader DirectX blob export

* [*AssetRipperGUI*](../AssetRipperGUI/README.md)

   Basic graphic interface application utilizing the AssetRipperLibrary.
   
* [*AssetRipperConsole*](../AssetRipperConsole/README.md)

   Command line equivalent of AssetRipperGUI. Since it has no GUI, it can be cross-platform compatible.
   
* [*UnitTester*](../UnitTester/README.md)

   Automated tester to verify project integrity while making changes.