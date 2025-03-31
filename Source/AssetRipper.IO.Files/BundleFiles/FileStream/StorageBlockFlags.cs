using AssetRipper.SmartEnums;

namespace AssetRipper.IO.Files.BundleFiles.FileStream;

[SmartEnum]
public readonly partial record struct StorageBlockFlags
{
	private enum Internal
	{
		CompressionTypeMask = 0x3F,

		Streamed = 0x40,
	}

	public CompressionType CompressionType
	{
		get
		{
			return (CompressionType)(this & CompressionTypeMask);
		}
	}

	public bool IsStreamed
	{
		get
		{
			return (this & Streamed) != 0;
		}
	}

	public StorageBlockFlags WithCompressionType(CompressionType compressionType)
	{
		return (this & ~CompressionTypeMask) | (StorageBlockFlags)compressionType;
	}

	public static explicit operator StorageBlockFlags(CompressionType compressionType)
	{
		return (StorageBlockFlags)(int)compressionType;
	}

	public static explicit operator CompressionType(StorageBlockFlags flags)
	{
		return (CompressionType)(int)(flags);
	}
}
