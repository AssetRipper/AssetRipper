namespace UtinyRipper.Classes.Shaders
{
	public struct TextureParameter : IAssetReadable
	{
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadMultiSampled(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}

		public TextureParameter(string name, int index, int dimension)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			Dim = (byte)dimension;
			SamplerIndex = dimension >> 8;
			if (SamplerIndex == 0xFFFFFF)
			{
				SamplerIndex = -1;
			}
			MultiSampled = false;
		}

		public TextureParameter(string name, int index, bool multiSampled, int dimension):
			this(name, index, dimension)
		{
			MultiSampled = multiSampled;
		}

		public void Read(AssetStream stream)
		{
			NameIndex = stream.ReadInt32();
			Index = stream.ReadInt32();
			SamplerIndex = stream.ReadInt32();

			if(IsReadMultiSampled(stream.Version))
			{
				MultiSampled = stream.ReadBoolean();
			}
			Dim = stream.ReadByte();
			stream.AlignStream(AlignType.Align4);
		}

		public string Name { get; private set; }
		public int NameIndex { get; private set; }
		public int Index { get; private set; }
		public int SamplerIndex { get; private set; }
		public bool MultiSampled { get; private set; }
		public byte Dim { get; private set; }
	}
}
