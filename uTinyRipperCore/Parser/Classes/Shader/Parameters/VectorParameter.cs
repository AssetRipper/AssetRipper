namespace uTinyRipper.Classes.Shaders
{
	public struct VectorParameter : IAssetReadable
	{
		public VectorParameter(string name, ShaderParamType type, int index, int columns)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			ArraySize = 0;
			Type = type;
			Dim = (byte)columns;
		}

		public VectorParameter(string name, ShaderParamType type, int index, int arraySize, int columns):
			this(name, type, index, columns)
		{
			ArraySize = arraySize;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
			ArraySize = reader.ReadInt32();
			Type = (ShaderParamType)reader.ReadByte();
			Dim = reader.ReadByte();
			reader.AlignStream(AlignType.Align4);
		}

		public string Name { get; private set; }
		public int NameIndex { get; private set; }
		public int Index { get; private set; }
		public int ArraySize { get; private set; }
		public ShaderParamType Type { get; private set; }
		public byte Dim { get; private set; }
	}
}
