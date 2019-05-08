using uTinyRipper.Assembly;

namespace uTinyRipper.WebFiles
{
	public sealed class WebFileScheme : FileSchemeList
	{
		private WebFileScheme(SmartStream stream, long offset, long size, string filePath, string fileName) :
			base(stream, offset, size, filePath, fileName)
		{
		}

		internal static WebFileScheme ReadScheme(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			WebFileScheme scheme = new WebFileScheme(stream, offset, size, filePath, fileName);
			scheme.ReadScheme();
			scheme.ProcessEntries();
			return scheme;
		}

		public WebFile ReadFile(IFileCollection collection, IAssemblyManager manager)
		{
			WebFile web = new WebFile(collection, this);
			foreach (FileScheme scheme in Schemes)
			{
				web.AddFile(scheme, collection, manager);
			}
			return web;
		}

		private void ReadScheme()
		{
			using (PartialStream stream = new PartialStream(m_stream, m_offset, m_size))
			{
				using (EndianReader reader = new EndianReader(stream, EndianType.LittleEndian))
				{
					Header.Read(reader);
					Metadata.Read(reader);
				}
			}
		}

		private void ProcessEntries()
		{
			foreach (WebFileEntry entry in Metadata.Entries.Values)
			{
				FileScheme scheme = FileCollection.ReadScheme(m_stream, entry.Offset, entry.Size, FilePath, entry.NameOrigin);
				AddScheme(scheme);
			}
		}

		public WebHeader Header { get; } = new WebHeader();
		public WebMetadata Metadata { get; } = new WebMetadata();

		public override FileEntryType SchemeType => FileEntryType.Web;
	}
}
