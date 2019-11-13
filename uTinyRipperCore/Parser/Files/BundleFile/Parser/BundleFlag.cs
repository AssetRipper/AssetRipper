using System;

namespace uTinyRipper.BundleFiles
{
	[Flags]
	public enum BundleFlag
	{
		CompressionMask		= 0x3F,

		HasEntryInfo		= 0x40,
		MetadataAtTheEnd	= 0x80,
	}

	internal static class BundleFlagExtensions
	{
		public static BundleCompressType GetCompression(this BundleFlag _this)
		{
			return (BundleCompressType)(_this & BundleFlag.CompressionMask);
		}
		
		public static bool IsHasEntryInfo(this BundleFlag _this)
		{
			return (_this & BundleFlag.HasEntryInfo) != 0;
		}
		
		public static bool IsMetadataAtTheEnd(this BundleFlag _this)
		{
			return (_this & BundleFlag.MetadataAtTheEnd) != 0;
		}
	}
}
