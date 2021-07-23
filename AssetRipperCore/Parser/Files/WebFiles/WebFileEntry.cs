using AssetRipper.IO.Endian;
using AssetRipper.Parser.Files.Entries;
using AssetRipper.Parser.Utils;

namespace AssetRipper.Parser.Files.WebFiles
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
