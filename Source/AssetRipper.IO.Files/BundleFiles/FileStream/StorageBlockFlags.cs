namespace AssetRipper.IO.Files.BundleFiles.FileStream;

[Flags]
public enum StorageBlockFlags
{
	CompressionTypeMask = 0x3F,

	Streamed = 0x40,
}
public static class StorageBlockFlagsExtensions
{
	extension(StorageBlockFlags flags)
	{
		public CompressionType CompressionType
		{
			get
			{
				return (CompressionType)(flags & StorageBlockFlags.CompressionTypeMask);
			}
		}

		public bool IsStreamed
		{
			get
			{
				return (flags & StorageBlockFlags.Streamed) != 0;
			}
		}

		public StorageBlockFlags WithCompressionType(CompressionType compressionType)
		{
			return (flags & ~StorageBlockFlags.CompressionTypeMask) | (StorageBlockFlags)compressionType;
		}
	}
}
