using System.Collections.Generic;

namespace uTinyRipper.BundleFiles
{
	/// <summary>
	/// Metadata about bundle's block or chunk
	/// </summary>
	public sealed class BundleMetadata : IBundleFileReadable
	{
		private static bool IsReadBlockInfo(BundleGeneration generation)
		{
			return generation >= BundleGeneration.BF_530_x;
		}

		public void Read(BundleFileReader reader)
		{
			if (IsReadBlockInfo(reader.Generation))
			{
				// unknown 0x10
				reader.BaseStream.Position += 0x10;
				BlockInfos = reader.ReadBundleArray<BlockInfo>();
			}

			int count = reader.ReadInt32();
			Dictionary<string, BundleFileEntry> entries = new Dictionary<string, BundleFileEntry>(count);
			for (int i = 0; i < count; i++)
			{
				BundleFileEntry entry = reader.ReadBundle<BundleFileEntry>();
				entries.Add(entry.Name, entry);
			}
			Entries = entries;
		}

		public IReadOnlyDictionary<string, BundleFileEntry> Entries { get; private set; }

		internal IReadOnlyList<BlockInfo> BlockInfos { get; private set; }
	}
}
