using AssetRipper.SourceGenerated.Extensions.Enums.Shader;
using System.Diagnostics;

namespace AssetRipper.Export.Modules.Shaders.ShaderBlob.Parameters;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class VectorParameter : NumericShaderParameter
{
	public VectorParameter() { }

	public VectorParameter(string name, ShaderParamType type, int index, int columns)
	{
		Name = name;
		NameIndex = -1;
		Index = index;
		ArraySize = 0;
		Type = type;
		Dim = (byte)columns;
		ColumnCount = 1;
		IsMatrix = false;
	}

	public VectorParameter(string name, ShaderParamType type, int index, int arraySize, int columns) : this(name, type, index, columns)
	{
		ArraySize = arraySize;
	}

	public byte Dim
	{
		get { return RowCount; }
		set { RowCount = value; }
	}

	private string? GetDebuggerDisplay()
	{
		return Name;
	}
}
