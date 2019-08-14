namespace uTinyRipper.Classes.Shaders
{
	public struct MatrixParameter : IAssetReadable
	{
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

		public MatrixParameter(string name, ShaderParamType type, int index, int arraySize, int rowCount, int columnCount) :
			this(name, type, index, rowCount, columnCount)
		{
			ArraySize = arraySize;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
			ArraySize = reader.ReadInt32();
			Type = (ShaderParamType)reader.ReadByte();
			RowCount = reader.ReadByte();
			ColumnCount = 0;
			reader.AlignStream(AlignType.Align4);
		}

		public string Name { get; private set; }
		public int NameIndex { get; private set; }
		public int Index { get; private set; }
		public int ArraySize { get; private set; }
		public ShaderParamType Type { get; private set; }
		public byte RowCount { get; private set; }
		public byte ColumnCount { get; private set; }
	}
}
