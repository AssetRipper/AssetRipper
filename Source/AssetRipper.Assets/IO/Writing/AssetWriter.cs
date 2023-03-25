using AssetRipper.Assets.Collections;
using AssetRipper.IO.Endian;

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

		public AssetCollection AssetCollection { get; }
	}
}
