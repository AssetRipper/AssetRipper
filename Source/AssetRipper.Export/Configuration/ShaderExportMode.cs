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
	/// Export as disassembly which does not compile in the editor
	/// </summary>
	Disassembly,
	/// <summary>
	/// Export as decompiled hlsl (unstable!)
	/// </summary>
	Decompile
}
