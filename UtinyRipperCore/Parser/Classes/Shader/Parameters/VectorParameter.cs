namespace UtinyRipper.Classes.Shaders
{
	public struct VectorParameter : IAssetReadable
	{
		public VectorParameter(string name, ShaderParamType type, int index, int dimension)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			ArraySize = 0;
			Type = type;
			Dim = (byte)dimension;
		}

		public VectorParameter(string name, ShaderParamType type, int index, int arraySize, int dimension):
			this(name, type, index, dimension)
		{
			ArraySize = arraySize;
		}

		public void Read(AssetStream stream)
		{
			NameIndex = stream.ReadInt32();
			Index = stream.ReadInt32();
			ArraySize = stream.ReadInt32();
			Type = (ShaderParamType)stream.ReadByte();
			Dim = stream.ReadByte();
			stream.AlignStream(AlignType.Align4);
		}

		public string Name { get; private set; }
		public int NameIndex { get; private set; }
		public int Index { get; private set; }
		public int ArraySize { get; private set; }
		public ShaderParamType Type { get; private set; }
		public byte Dim { get; private set; }
	}
}
