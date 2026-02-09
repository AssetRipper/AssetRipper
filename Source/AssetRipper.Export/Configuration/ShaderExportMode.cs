namespace AssetRipper.Export.Configuration;

public enum ShaderExportMode
{
	/// <summary>
	/// Export as dummy shaders which compile in the editor
	/// </summary>
	Dummy,
	/// <summary>
	/// Export as yaml assets which can be viewed in the editor
	/// </summary>
	Yaml,
	/// <summary>
	/// Export as decompiled HLSL
	/// </summary>
	Decompile
}
