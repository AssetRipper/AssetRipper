namespace uTinyRipper.Classes.Shaders
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

		public TextureParameter(string name, int index, int samplerIndex)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			Dim = 2;
			SamplerIndex = samplerIndex;
			MultiSampled = false;
		}

		public TextureParameter(string name, int index, int samplerIndex, bool multiSampled, byte dimension):
			this(name, index, samplerIndex)
		{
			MultiSampled = multiSampled;
			Dim = dimension;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
			SamplerIndex = reader.ReadInt32();

			if(IsReadMultiSampled(reader.Version))
			{
				MultiSampled = reader.ReadBoolean();
			}
			Dim = reader.ReadByte();
			reader.AlignStream(AlignType.Align4);
		}

		public string Name { get; private set; }
		public int NameIndex { get; private set; }
		public int Index { get; private set; }
		public int SamplerIndex { get; private set; }
		public bool MultiSampled { get; private set; }
		public byte Dim { get; private set; }
	}
}
