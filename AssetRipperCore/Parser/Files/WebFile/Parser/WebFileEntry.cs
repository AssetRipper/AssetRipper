using AssetRipper.IO.Endian;
using AssetRipper.Parser.Files.File.Parser;
using AssetRipper.Parser.Utils;

namespace AssetRipper.Parser.Files.WebFile.Parser
{
	public class WebFileEntry : FileEntry, IEndianReadable
	{
		public void Read(EndianReader reader)
		{
			Offset = reader.ReadInt32();
			Size = reader.ReadInt32();
			NameOrigin = reader.ReadString();
			Name = FilenameUtils.FixFileIdentifier(NameOrigin);
		}
	}
}
