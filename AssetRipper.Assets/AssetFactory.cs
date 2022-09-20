using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets;

public abstract class AssetFactory
{
	public abstract IUnityObjectBase? ReadAsset(AssetInfo assetInfo, EndianReader reader, int size, SerializedType type);
}

