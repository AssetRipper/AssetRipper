using AssetRipper.Core.Classes.Shader.Enums;

namespace ShaderTextRestorer.ShaderBlob.Parameters
{
	public sealed class MatrixParameter : NumericShaderParameter
	{
		public MatrixParameter() { }

		public MatrixParameter(string name, ShaderParamType type, int index, int rowCount, int columnCount)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			ArraySize = 0;
			Type = type;
			RowCount = (byte)rowCount;
			ColumnCount = (byte)columnCount;
			IsMatrix = true;
		}

		public MatrixParameter(string name, ShaderParamType type, int index, int arraySize, int rowCount, int columnCount) : this(name, type, index, rowCount, columnCount)
		{
			ArraySize = arraySize;
		}
	}
}
