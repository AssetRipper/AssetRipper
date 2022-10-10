using AssetRipper.Assets.Collections;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.VersionUtilities;
using System.IO;

namespace AssetRipper.Assets.IO.Writing
{
	public sealed class AssetWriter : EndianWriter
	{
		public AssetWriter(Stream stream, AssetCollection assetCollection) : base(stream, assetCollection.EndianType, false)
		{
			AssetCollection = assetCollection;
		}

		public override void Write(string value)
		{
			throw new NotSupportedException();
		}

		public void WriteAsset<T>(T value) where T : IAssetWritable
		{
			value.Write(this);
		}

		public UnityVersion Version => AssetCollection.Version;
		public BuildTarget Platform => AssetCollection.Platform;
		public TransferInstructionFlags Flags => AssetCollection.Flags;
		public AssetCollection AssetCollection { get; }
	}
}
