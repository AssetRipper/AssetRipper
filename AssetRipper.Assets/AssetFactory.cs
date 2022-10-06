using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets;

public abstract class AssetFactory
{
	public abstract IUnityObjectBase? ReadAsset(AssetInfo assetInfo, AssetReader reader, int size, SerializedType type);
}
