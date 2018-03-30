using System.IO;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.WebFiles
{
	internal class WebFileEntry : FileEntry
	{
		public WebFileEntry(Stream stream, string name, long offset, long size):
			base(stream, name, offset, size)
		{
		}

		public void ReadFile(AssetCollection collection, string filePath)
		{
			m_stream.Position = m_offset;
#warning TODO: replace resource/unity_default_resources to library/unity default resources ???;
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
