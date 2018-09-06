using System;
using System.IO;
using UtinyRipper.AssetExporters;

namespace UtinyRipper.BundleFiles
{
	public class BundleFileEntry : FileEntry
	{
		public BundleFileEntry(Stream stream, string filePath, string name, long offset, long size, bool isStreamPermanent) :
			base(stream, filePath, name, offset, size, isStreamPermanent)
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
