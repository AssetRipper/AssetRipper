using System.Collections.Generic;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.ResourceFiles
{
	public sealed class ResourceFileScheme : FileScheme
	{
		public ResourceFileScheme(SmartStream stream, long offset, long size, string filePath, string fileName):
			base(stream, offset, size, filePath, fileName)
		{
		}

		internal static ResourceFileScheme ReadScheme(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			return new ResourceFileScheme(stream, offset, size, filePath, fileName);
		}

		public ResourceFile ReadFile()
		{
			return new ResourceFile(m_stream, m_offset, m_size, FilePath, NameOrigin);
		}

		public override bool ContainsFile(string fileName)
		{
			return false;
		}

		public override FileEntryType SchemeType => FileEntryType.Resource;
		public override IEnumerable<FileIdentifier> Dependencies { get { yield break; } }
	}
}
