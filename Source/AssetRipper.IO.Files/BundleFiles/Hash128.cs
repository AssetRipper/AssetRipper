using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles;

public sealed record class Hash128 : IEndianReadable<Hash128>, IEndianWritable
{
	public static Hash128 Read(EndianReader reader)
	{
		return new()
		{
			Byte0 = reader.ReadByte(),
			Byte1 = reader.ReadByte(),
			Byte2 = reader.ReadByte(),
			Byte3 = reader.ReadByte(),
			Byte4 = reader.ReadByte(),
			Byte5 = reader.ReadByte(),
			Byte6 = reader.ReadByte(),
			Byte7 = reader.ReadByte(),
			Byte8 = reader.ReadByte(),
			Byte9 = reader.ReadByte(),
			Byte10 = reader.ReadByte(),
			Byte11 = reader.ReadByte(),
			Byte12 = reader.ReadByte(),
			Byte13 = reader.ReadByte(),
			Byte14 = reader.ReadByte(),
			Byte15 = reader.ReadByte(),
		};
	}

	public void Write(EndianWriter writer)
	{
		writer.Write(Byte0);
		writer.Write(Byte1);
		writer.Write(Byte2);
		writer.Write(Byte3);
		writer.Write(Byte4);
		writer.Write(Byte5);
		writer.Write(Byte6);
		writer.Write(Byte7);
		writer.Write(Byte8);
		writer.Write(Byte9);
		writer.Write(Byte10);
		writer.Write(Byte11);
		writer.Write(Byte12);
		writer.Write(Byte13);
		writer.Write(Byte14);
		writer.Write(Byte15);
	}

	public override string ToString()
	{
		return new UnityGuid(ToArray()).ToString();
	}

	public byte[] ToArray()
	{
		return
		[
			Byte0,
			Byte1,
			Byte2,
			Byte3,
			Byte4,
			Byte5,
			Byte6,
			Byte7,
			Byte8,
			Byte9,
			Byte10,
			Byte11,
			Byte12,
			Byte13,
			Byte14,
			Byte15,
		];
	}

	public byte Byte0 { get; set; }
	public byte Byte1 { get; set; }
	public byte Byte2 { get; set; }
	public byte Byte3 { get; set; }
	public byte Byte4 { get; set; }
	public byte Byte5 { get; set; }
	public byte Byte6 { get; set; }
	public byte Byte7 { get; set; }
	public byte Byte8 { get; set; }
	public byte Byte9 { get; set; }
	public byte Byte10 { get; set; }
	public byte Byte11 { get; set; }
	public byte Byte12 { get; set; }
	public byte Byte13 { get; set; }
	public byte Byte14 { get; set; }
	public byte Byte15 { get; set; }
}
