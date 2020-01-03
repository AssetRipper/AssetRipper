using System.Collections.Generic;

namespace uTinyRipper.WebFiles
{
	public class WebMetadata : IEndianReadable
	{
		public void Read(EndianReader reader)
		{
			List<WebFileEntry> entries = new List<WebFileEntry>();
			long metadataLength = reader.ReadInt32();
			while (reader.BaseStream.Position < metadataLength)
			{
				WebFileEntry entry = new WebFileEntry();
				entry.Read(reader);
				entries.Add(entry);
			}
			Entries = entries.ToArray();
		}

		public WebFileEntry[] Entries { get; set; }
	}
}
