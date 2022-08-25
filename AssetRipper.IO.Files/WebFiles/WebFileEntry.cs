using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.IO.Files.WebFiles
{
	public class WebFileEntry : IEndianReadable, IEndianWritable
	{
		public void Read(EndianReader reader)
		{
			Offset = reader.ReadInt32();
			Size = reader.ReadInt32();
			NameOrigin = reader.ReadString();
			Name = FilenameUtils.FixFileIdentifier(NameOrigin);
		}

		public void Write(EndianWriter writer)
		{
			writer.Write(Offset);
			writer.Write(Size);
			writer.Write(NameOrigin);
		}

		public override string? ToString()
		{
			return Name;
		}

		public int Offset { get; protected set; }
		public int Size { get; protected set; }
		public string NameOrigin { get; protected set; } = "";
		public string Name { get; protected set; } = "";
	}
}
