using AssetRipper.SourceGenerated.Extensions.Enums.Shader;
using System.Diagnostics;

namespace AssetRipper.Export.Modules.Shaders.ShaderBlob.Parameters;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class NumericShaderParameter
{
	public string? Name { get; set; }
	public int NameIndex { get; set; }
	public int Index { get; set; }
	public int ArraySize { get; set; }
	public ShaderParamType Type { get; set; }
	public byte RowCount { get; set; }
	public byte ColumnCount { get; set; }
	public bool IsMatrix { get; set; }

	private string? GetDebuggerDisplay()
	{
		return Name;
	}
}
