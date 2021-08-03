using AssetRipper.Core.Parser.Files.BundleFile.IO;

namespace AssetRipper.Core.Parser.Files.BundleFile.Parser
{
	/// <summary>
	/// Contains compression information about a block<br/>
	/// Blocks are similar to chunk structure in that it contains a data blob but without file entries
	/// </summary>
	public struct StorageBlock : IBundleReadable
	{
		public void Read(BundleReader reader)
		{
			UncompressedSize = reader.ReadUInt32();
			CompressedSize = reader.ReadUInt32();
			Flags = (StorageBlockFlags)reader.ReadUInt16();
		}

		public override string ToString()
		{
			return $"C:{CompressedSize} D:{UncompressedSize} F:{Flags}";
		}

		public uint UncompressedSize { get; private set; }
		public uint CompressedSize { get; private set; }
		public StorageBlockFlags Flags { get; private set; }
	}
}
