namespace UtinyRipper.BundleFiles
{
	/// <summary>
	/// Contains compression information about chunk
	/// Chunk is a structure (optionaly LZMA compressed) that contains file entries and data blob
	/// </summary>
	internal struct ChunkInfo : IEndianReadable
	{
		public void Read(EndianStream stream)
		{
			CompressedSize = stream.ReadUInt32();
			DecompressedSize = stream.ReadUInt32();
		}

		public uint CompressedSize { get; private set; }
		public uint DecompressedSize { get; private set; }
	}
}
