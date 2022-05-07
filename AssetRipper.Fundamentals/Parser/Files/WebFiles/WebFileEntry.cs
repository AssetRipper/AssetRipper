﻿using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Files.Entries;
using AssetRipper.Core.Parser.Utils;

namespace AssetRipper.Core.Parser.Files.WebFiles
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
