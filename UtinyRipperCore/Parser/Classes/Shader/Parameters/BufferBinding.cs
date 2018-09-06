namespace UtinyRipper.Classes.Shaders
{
	public struct BufferBinding : IAssetReadable
	{
		public BufferBinding(string name, int index)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
		}

		public string Name { get; private set; }
		public int NameIndex { get; private set; }
		public int Index { get; private set; }
	}
}
