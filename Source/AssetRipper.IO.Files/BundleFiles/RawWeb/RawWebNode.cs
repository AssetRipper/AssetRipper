using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles.RawWeb;

public sealed record class RawWebNode : Node, IEndianReadable<RawWebNode>
{
	public static RawWebNode Read(EndianReader reader)
	{
		return new()
		{
			Path = reader.ReadStringZeroTerm(),
			Offset = reader.ReadInt32(),
			Size = reader.ReadInt32(),
		};
	}

	public override void Write(EndianWriter writer)
	{
		writer.WriteStringZeroTerm(Path);
		writer.Write((int)Offset);
		writer.Write((int)Size);
	}
}
