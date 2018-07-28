using System.IO;

namespace UtinyRipper.WebFiles
{
	internal class WebFileEntry : FileEntry
	{
		public WebFileEntry(Stream stream, string name, long offset, long size, bool isStreamPermanent):
			base(stream, name, offset, size, isStreamPermanent)
		{
		}

		public void ReadFile(FileCollection collection, string filePath)
		{
			m_stream.Position = m_offset;
			collection.Read(m_stream, filePath, Name);
		}
		
		public override bool IsSkipFile
		{
			get
			{
				if(base.IsSkipFile)
				{
					return true;
				}

				string ext = Path.GetExtension(Name);
				switch (ext)
				{
					case ConfigExtention:
						return true;
					case DatExtention:
						return true;
					case XmlExtention:
						return true;

					default:
						return false;
				}
			}
		}

		private const string ConfigExtention = ".config";
		private const string DatExtention = ".dat";
		private const string XmlExtention = ".xml";
	}
}
