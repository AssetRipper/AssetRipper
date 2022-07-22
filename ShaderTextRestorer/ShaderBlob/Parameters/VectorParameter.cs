using AssetRipper.Core.Classes.Shader.Enums;

namespace ShaderTextRestorer.ShaderBlob.Parameters
{
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
	}
}
