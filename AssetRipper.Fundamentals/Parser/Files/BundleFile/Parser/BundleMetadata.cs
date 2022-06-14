using AssetRipper.Core.Parser.Files.BundleFile.IO;

namespace AssetRipper.Core.Parser.Files.BundleFile.Parser
{
	/// <summary>
	/// Metadata about bundle's block or chunk
	/// </summary>
	public sealed class BundleMetadata : IBundleReadable
	{
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		private static bool HasBlocksInfo(BundleType signature) => signature == BundleType.UnityFS;

		public void Read(BundleReader reader)
		{
			if (HasBlocksInfo(reader.Signature))
			{
				BlocksInfo.Read(reader);
				if (reader.Flags.GetBlocksAndDirectoryInfoCombined())
				{
					DirectoryInfo.Read(reader);
				}
			}
			else
			{
				DirectoryInfo.Read(reader);
				reader.AlignStream();
			}
		}

		public BlocksInfo BlocksInfo = new();
		public DirectoryInfo DirectoryInfo = new();
	}
}
