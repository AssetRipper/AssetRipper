using System;

namespace uTinyRipper.BundleFiles
{
	[Flags]
	public enum BundleFlags
	{
		CompressionTypeMask					= 0x3F,

		BlocksAndDirectoryInfoCombined		= 0x40,
		BlocksInfoAtTheEnd					= 0x80,
		OldWebPluginCompatibility			= 0x100,
	}

	public static class BundleFlagsExtensions
	{
		public static CompressionType GetCompression(this BundleFlags _this)
		{
			return (CompressionType)(_this & BundleFlags.CompressionTypeMask);
		}
		
		public static bool IsBlocksAndDirectoryInfoCombined(this BundleFlags _this)
		{
			return (_this & BundleFlags.BlocksAndDirectoryInfoCombined) != 0;
		}
		
		public static bool IsBlocksInfoAtTheEnd(this BundleFlags _this)
		{
			return (_this & BundleFlags.BlocksInfoAtTheEnd) != 0;
		}
	}
}
