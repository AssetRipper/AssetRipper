namespace UtinyRipper.BundleFiles
{
	/// <summary>
	/// Contains compression information about block
	/// Block is a similar to chunk structure that contains data blob but without file entries
	/// </summary>
	internal struct BlockInfo : IEndianReadable
	{
		public void Read(EndianReader stream)
		{
			DecompressedSize = stream.ReadUInt32();
			CompressedSize = stream.ReadUInt32();
			Flags = (BundleFlag)stream.ReadUInt16();
		}

		public uint DecompressedSize { get; private set; }
		public uint CompressedSize { get; private set; }
		public BundleFlag Flags { get; private set; }
	}
}
