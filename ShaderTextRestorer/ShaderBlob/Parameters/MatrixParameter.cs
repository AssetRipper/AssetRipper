using AssetRipper.Core.Classes.Shader.Enums;

namespace ShaderTextRestorer.ShaderBlob.Parameters
{
	public sealed class MatrixParameter
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
		}

		public MatrixParameter(string name, ShaderParamType type, int index, int arraySize, int rowCount, int columnCount) : this(name, type, index, rowCount, columnCount)
		{
			ArraySize = arraySize;
		}

		public string Name { get; set; } = string.Empty;
		public int NameIndex { get; set; }
		public int Index { get; set; }
		public int ArraySize { get; set; }
		public ShaderParamType Type { get; set; }
		public byte RowCount { get; set; }
		public byte ColumnCount { get; set; }
	}
}
