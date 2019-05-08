namespace uTinyRipper.BundleFiles
{
	/// <summary>
	/// Contains compression information about block
	/// Block is a similar to chunk structure that contains data blob but without file entries
	/// </summary>
	internal struct BlockInfo : IBundleFileReadable
	{
		public void Read(BundleFileReader reader)
		{
			DecompressedSize = reader.ReadUInt32();
			CompressedSize = reader.ReadUInt32();
			Flags = (BundleFlag)reader.ReadUInt16();
		}

		public override string ToString()
		{
			return $"C:{CompressedSize} D:{DecompressedSize} F:{Flags}";
		}

		public uint DecompressedSize { get; private set; }
		public uint CompressedSize { get; private set; }
		public BundleFlag Flags { get; private set; }
	}
}
