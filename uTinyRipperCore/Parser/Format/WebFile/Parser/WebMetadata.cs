using System.Collections.Generic;

namespace uTinyRipper.WebFiles
{
	public class WebMetadata : IEndianReadable
	{
		public void Read(EndianReader reader)
		{
			Dictionary<string, WebFileEntry> entries = new Dictionary<string, WebFileEntry>();
			long metadataLength = reader.ReadInt32();
			while (reader.BaseStream.Position < metadataLength)
			{
				WebFileEntry entry = new WebFileEntry();
				entry.Read(reader);
				entries.Add(entry.Name, entry);
			}
			Entries = entries;
		}

		public IReadOnlyDictionary<string, WebFileEntry> Entries { get; private set; }
	}
}
