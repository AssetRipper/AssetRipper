using AssetRipper.Assets.Collections;
using AssetRipper.IO.Endian;

namespace AssetRipper.Assets.IO.Writing;

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

	public void WriteAssetArray<T>(T[] buffer) where T : IAssetWritable
	{
		Write(buffer.Length);

		for (int i = 0; i < buffer.Length; i++)
		{
			buffer[i].Write(this);
		}

		if (IsAlignArray)
		{
			AlignStream();
		}
	}

	public AssetCollection AssetCollection { get; }
}
