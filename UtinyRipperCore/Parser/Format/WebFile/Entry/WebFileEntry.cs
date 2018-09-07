using System;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.BundleFiles;

namespace UtinyRipper.WebFiles
{
	public class WebFileEntry : FileEntry
	{
		internal WebFileEntry(SmartStream stream, string filePath, string name, long offset, long size):
			base(stream, filePath, name, offset, size)
		{
			if(IsResourceFile)
			{
				EntryType = FileEntryType.Resource;
			}
			else if (IsSerializedFile)
			{
				EntryType = FileEntryType.Serialized;
			}
			else if (IsBundleFile)
			{
				EntryType = FileEntryType.Bundle;
			}
			else if (IsWebFile)
			{
				EntryType = FileEntryType.Web;
			}
			else if(IsSkipFile)
			{
				EntryType = FileEntryType.Unknown;
			}
			else
			{
				throw new Exception($"Unsupport {nameof(WebFile)} entry {Name} for file '{filePath}'");
			}
		}

		public override FileEntryType EntryType { get; }

		protected override bool IsSerializedFileInner
		{
			get
			{
				if (IsBundleFile)
				{
					return false;
				}
				if (IsWebFile)
				{
					return false;
				}
				return true;
			}
		}

		protected override bool IsResourceFileInner
		{
			get
			{
				return AssemblyManager.IsAssembly(Name);
			}
		}

		protected override bool IsSkipFileInner
		{
			get
			{
				string ext = Path.GetExtension(Name);
				switch (ext)
				{
					case ConfigExtention:
					case DatExtention:
					case XmlExtention:
						return true;

					default:
						return false;
				}
			}
		}

		private bool IsBundleFile
		{
			get
			{
				using (PartialStream stream = new PartialStream(m_stream, m_offset, m_size))
				{
					return BundleFile.IsBundleFile(stream);
				}
			}
		}

		private bool IsWebFile
		{
			get
			{
				using (PartialStream stream = new PartialStream(m_stream, m_offset, m_size))
				{
					return WebFile.IsWebFile(stream);
				}
			}
		}

		private const string ConfigExtention = ".config";
		private const string DatExtention = ".dat";
		private const string XmlExtention = ".xml";
	}
}
