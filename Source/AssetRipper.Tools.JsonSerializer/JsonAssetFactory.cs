using AssetRipper.Assets;
using AssetRipper.Assets.Exceptions;
using AssetRipper.Assets.IO;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Tools.JsonSerializer;

public sealed class JsonAssetFactory : AssetFactoryBase
{
	public override IUnityObjectBase? ReadAsset(AssetInfo assetInfo, ref EndianSpanReader reader, TransferInstructionFlags flags, int size, SerializedType? type)
	{
		if (type?.OldType.Nodes.Count > 0)
		{
			SerializableEntry entry = SerializableEntry.FromTypeTree(type.OldType);
			JsonAsset asset = new JsonAsset(assetInfo);
			asset.Read(ref reader, entry);
			IncorrectByteCountException.ThrowIf(in reader);
			return asset;
		}
		else
		{
			Console.WriteLine($"Asset could not be read because it has no type tree. ClassID: {assetInfo.ClassID} PathID: {assetInfo.PathID}");
			return null;
		}
	}
}
