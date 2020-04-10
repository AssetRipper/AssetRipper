using uTinyRipper.Classes.Misc;

namespace uTinyRipper.BundleFiles
{
	public sealed class BundleRawWebHeader
	{
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasHash(BundleVersion generation) => generation >= BundleVersion.BF_520a1;
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasCompleteFileSize(BundleVersion generation) => generation >= BundleVersion.BF_260_340;
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasUncompressedBlocksInfoSize(BundleVersion generation) => generation >= BundleVersion.BF_350_4x;

		public void Read(EndianReader reader, BundleVersion generation)
		{
			if (HasHash(generation))
			{
				Hash.Read(reader);
				Crc = reader.ReadUInt32();
			}
			MinimumStreamedBytes = reader.ReadUInt32();
			HeaderSize = reader.ReadInt32();
			NumberOfScenesToDownloadBeforeStreaming = reader.ReadInt32();
			Scenes = reader.ReadEndianArray<BundleScene>();
			if (HasCompleteFileSize(generation))
			{
				CompleteFileSize = reader.ReadUInt32();
			}
			if (HasUncompressedBlocksInfoSize(generation))
			{
				UncompressedBlocksInfoSize = (int)reader.ReadUInt32();
			}
			reader.AlignStream();
		}

		public uint Crc { get; set; }
		/// <summary>
		/// Minimum number of bytes to read for streamed bundles, equal to BundleSize for normal bundles
		/// </summary>
		public uint MinimumStreamedBytes { get; set; }
		public int HeaderSize { get; set; }
		/// <summary>
		/// Equal to 1 if it's a streamed bundle, number of LZMAChunkInfos + mainData assets otherwise
		/// </summary>
		public int NumberOfScenesToDownloadBeforeStreaming { get; set; }
		/// <summary>
		/// LZMA chunks info
		/// </summary>
		public BundleScene[] Scenes { get; set; }
		public uint CompleteFileSize { get; set; }
		public int UncompressedBlocksInfoSize { get; set; }

		public Hash128 Hash;
	}
}
