using System;
using System.IO;

namespace UtinyRipper.WebFiles
{
	internal class WebFileEntry : FileEntry
	{
		public WebFileEntry(Stream stream, string filePath, string name, long offset, long size, bool isStreamPermanent):
			base(stream, filePath, name, offset, size, isStreamPermanent)
		{
		}

		public void ReadFile(FileCollection collection, Action<string> requestDependencyCallback)
		{
			m_stream.Position = m_offset;
			collection.Read(m_stream, Name, m_filePath, requestDependencyCallback);
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
