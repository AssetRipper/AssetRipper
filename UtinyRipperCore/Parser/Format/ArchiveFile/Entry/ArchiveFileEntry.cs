using System;
using UtinyRipper.BundleFiles;
using UtinyRipper.WebFiles;

namespace UtinyRipper.ArchiveFiles
{
	public sealed class ArchiveFileEntry : FileEntry
	{
		public ArchiveFileEntry(SmartStream stream, string filePath, string name, long offset, long size) :
			base(stream, filePath, name, offset, size)
		{
			if (IsSerializedFile)
			{
				EntryType = FileEntryType.Serialized;
			}
			else if(IsBundleFile)
			{
				EntryType = FileEntryType.Bundle;
			}
			else if (IsWebFile)
			{
				EntryType = FileEntryType.Web;
			}
			else
			{
				throw new Exception($"Unsupport {nameof(ArchiveFile)} entry {Name} for file '{filePath}'");
			}
		}

		public void ReadSerializedFile(FileCollection collection)
		{
			ReadSerializedFile(collection, collection.OnRequestDependency);
		}

		public override FileEntryType EntryType { get; }

		protected override bool IsSerializedFileInner
		{
			get
			{
				if(IsBundleFile)
				{
					return false;
				}
				if(IsWebFile)
				{
					return false;
				}
				return true;
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
	}
}
