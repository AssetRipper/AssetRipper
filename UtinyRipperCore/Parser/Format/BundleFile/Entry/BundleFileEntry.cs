using System;
using UtinyRipper.AssetExporters;

namespace UtinyRipper.BundleFiles
{
	public class BundleFileEntry : FileEntry
	{
		internal BundleFileEntry(BundleFileEntry copy, long offset):
			this(copy.m_stream, copy.FilePath, copy.Name, offset, copy.Size)
		{
		}

		internal BundleFileEntry(SmartStream stream, string filePath, string name, long offset, long size) :
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
			else if (IsSkipFile)
			{
				EntryType = FileEntryType.Unknown;
			}
			else
			{
				throw new Exception($"Unsupport {nameof(BundleFile)} entry {Name} for file '{filePath}'");
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
				return Name.EndsWith(ManifestExtention, StringComparison.Ordinal);
			}
		}

		public override FileEntryType EntryType { get; }

		public long Offset => m_offset;
		public long Size => m_size;

		private const string ManifestExtention = ".manifest";
	}
}
