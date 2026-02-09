using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.WebFiles;

public sealed class WebFileEntry : IEndianReadable<WebFileEntry>, IEndianWritable
{
	public static WebFileEntry Read(EndianReader reader)
	{
		return new()
		{
			Offset = reader.ReadInt32(),
			Size = reader.ReadInt32(),
			Name = reader.ReadString()
		};
	}

	public void Write(EndianWriter writer)
	{
		writer.Write(Offset);
		writer.Write(Size);
		writer.Write(Name);
	}

	public override string ToString() => SpecialFileNames.FixFileIdentifier(Name);

	public int Offset { get; private set; }
	public int Size { get; private set; }
	public string Name { get; private set; } = "";
}
