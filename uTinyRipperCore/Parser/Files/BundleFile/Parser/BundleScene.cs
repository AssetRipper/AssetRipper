namespace uTinyRipper.BundleFiles
{
	/// <summary>
	/// Contains compression information about chunk
	/// Chunk is a structure (optionaly LZMA compressed) that contains file entries and data blob
	/// </summary>
	public struct BundleScene : IEndianReadable
	{
		public void Read(EndianReader reader)
		{
			CompressedSize = reader.ReadUInt32();
			DecompressedSize = reader.ReadUInt32();
		}

		public override string ToString()
		{
			return $"C:{CompressedSize} D:{DecompressedSize}";
		}

		public uint CompressedSize { get; private set; }
		public uint DecompressedSize { get; private set; }
	}
}
