using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Entries;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.IO.Files.WebFiles
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
