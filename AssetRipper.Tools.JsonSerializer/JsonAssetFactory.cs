using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Tools.JsonSerializer;

public sealed class JsonAssetFactory : AssetFactory
{
	public override IUnityObjectBase? ReadAsset(AssetInfo assetInfo, EndianReader reader, int size, SerializedType type)
	{
		if (type.OldType.Nodes.Count > 0)
		{
			SerializableEntry entry = SerializableEntry.FromTypeTree(type.OldType);
			JsonAsset asset = new JsonAsset(assetInfo);
			asset.Read(reader, entry);
			return asset;
		}
		else
		{
			return null;
		}
	}
}
