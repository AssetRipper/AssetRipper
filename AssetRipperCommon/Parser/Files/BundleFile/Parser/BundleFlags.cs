using System;

namespace AssetRipper.Core.Parser.Files.BundleFile.Parser
{
	[Flags]
	public enum BundleFlags
	{
		CompressionTypeMask = 0x3F,

		BlocksAndDirectoryInfoCombined = 0x40,
		BlocksInfoAtTheEnd = 0x80,
		OldWebPluginCompatibility = 0x100,
	}

	public static class BundleFlagsExtensions
	{
		/// <summary>
		/// The lowest 6 bits
		/// </summary>
		public static CompressionType GetCompression(this BundleFlags _this)
		{
			return (CompressionType)(_this & BundleFlags.CompressionTypeMask);
		}

		/// <summary>
		/// The 0x40 bit
		/// </summary>
		public static bool IsBlocksAndDirectoryInfoCombined(this BundleFlags _this)
		{
			return (_this & BundleFlags.BlocksAndDirectoryInfoCombined) != 0;
		}

		/// <summary>
		/// The 0x80 bit
		/// </summary>
		public static bool IsBlocksInfoAtTheEnd(this BundleFlags _this)
		{
			return (_this & BundleFlags.BlocksInfoAtTheEnd) != 0;
		}
	}
}
