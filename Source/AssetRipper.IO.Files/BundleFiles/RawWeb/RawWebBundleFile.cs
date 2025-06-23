using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.BundleFiles.RawWeb.Raw;
using AssetRipper.IO.Files.BundleFiles.RawWeb.Web;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.BundleFiles.RawWeb;

public abstract class RawWebBundleFile<THeader> : FileContainer where THeader : RawWebBundleHeader, new()
{
	public THeader Header { get; } = new();
	public DirectoryInfo<RawWebNode> DirectoryInfo { get; set; } = new();

	public override void Read(SmartStream stream)
	{
		EndianReader reader = new EndianReader(stream, EndianType.BigEndian);
		long basePosition = stream.Position;
		Header.Read(reader);
		long headerSize = stream.Position - basePosition;
		if (headerSize != Header.HeaderSize)
		{
			throw new Exception($"Read {headerSize} but expected {Header.HeaderSize} bytes while reading the raw/web bundle header.");
		}
		ReadRawWebMetadata(stream, out Stream dataStream, out long metadataOffset);//ReadBlocksAndDirectory
		ReadRawWebData(dataStream, metadataOffset);//also ReadBlocksAndDirectory
	}

	public override void Write(Stream stream)
	{
		EndianWriter writer = new EndianWriter(stream, EndianType.BigEndian);
		Header.Write(writer);
		throw new NotImplementedException();
	}

	private void ReadRawWebMetadata(Stream stream, out Stream dataStream, out long metadataOffset)
	{
		int metadataSize = RawWebBundleHeader.HasUncompressedBlocksInfoSize(Header.Version) ? Header.UncompressedBlocksInfoSize : 0;

		//These branches are collapsed by JIT
		if (typeof(THeader) == typeof(RawBundleHeader))
		{
			dataStream = stream;
			metadataOffset = stream.Position;
			ReadMetadata(dataStream, metadataSize);
		}
		else if (typeof(THeader) == typeof(WebBundleHeader))
		{
			// read only last chunk
			BundleScene chunkInfo = Header.Scenes[^1];
			dataStream = new MemoryStream(new byte[chunkInfo.DecompressedSize]);
			LzmaCompression.DecompressLzmaSizeStream(stream, chunkInfo.CompressedSize, dataStream);
			metadataOffset = 0;

			dataStream.Position = 0;
			ReadMetadata(dataStream, metadataSize);
		}
		else
		{
			throw new Exception($"Unsupported bundle type '{typeof(THeader)}'");
		}
	}

	private void ReadMetadata(Stream stream, int metadataSize)
	{
		long metadataPosition = stream.Position;
		using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
		{
			DirectoryInfo = DirectoryInfo<RawWebNode>.Read(reader);
			reader.AlignStream();
		}
		if (metadataSize > 0)
		{
			if (stream.Position - metadataPosition != metadataSize)
			{
				throw new Exception($"Read {stream.Position - metadataPosition} but expected {metadataSize} while reading bundle metadata");
			}
		}
	}

	private void ReadRawWebData(Stream stream, long metadataOffset)
	{
		foreach (RawWebNode entry in DirectoryInfo.Nodes)
		{
			byte[] buffer = new byte[entry.Size];
			stream.Position = metadataOffset + entry.Offset;
			stream.ReadExactly(buffer);
			ResourceFile file = new ResourceFile(buffer, FilePath, entry.Path);
			AddResourceFile(file);
		}
	}
}
