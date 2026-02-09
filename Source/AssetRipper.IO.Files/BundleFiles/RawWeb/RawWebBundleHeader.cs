using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles.RawWeb;

public abstract record class RawWebBundleHeader : BundleHeader
{
	public Hash128? Hash { get; set; }
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
	public BundleScene[] Scenes { get; set; } = Array.Empty<BundleScene>();
	public uint CompleteFileSize { get; set; }
	public int UncompressedBlocksInfoSize { get; set; }

	public sealed override void Read(EndianReader reader)
	{
		base.Read(reader);
		if (HasHash(Version))
		{
			Hash = reader.ReadEndian<Hash128>();
			Crc = reader.ReadUInt32();
		}
		MinimumStreamedBytes = reader.ReadUInt32();
		HeaderSize = reader.ReadInt32();
		NumberOfScenesToDownloadBeforeStreaming = reader.ReadInt32();
		Scenes = reader.ReadEndianArray<BundleScene>();
		if (HasCompleteFileSize(Version))
		{
			CompleteFileSize = reader.ReadUInt32();
		}
		if (HasUncompressedBlocksInfoSize(Version))
		{
			UncompressedBlocksInfoSize = (int)reader.ReadUInt32();
		}
		reader.AlignStream();
	}

	public sealed override void Write(EndianWriter writer)
	{
		base.Write(writer);
		throw new NotImplementedException();
	}

	/// <summary>
	/// 5.2.0 and greater / Bundle Version 4 +
	/// </summary>
	public static bool HasHash(BundleVersion generation) => generation >= BundleVersion.BF_520a1;
	/// <summary>
	/// 2.6.0 and greater / Bundle Version 2 +
	/// </summary>
	public static bool HasCompleteFileSize(BundleVersion generation) => generation >= BundleVersion.BF_260_340;
	/// <summary>
	/// 3.5.0 and greater / Bundle Version 3 +
	/// </summary>
	public static bool HasUncompressedBlocksInfoSize(BundleVersion generation) => generation >= BundleVersion.BF_350_4x;
}
