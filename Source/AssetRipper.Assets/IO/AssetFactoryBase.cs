using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets.IO;

public abstract class AssetFactoryBase
{
	public abstract IUnityObjectBase? ReadAsset(AssetInfo assetInfo, ref EndianSpanReader reader, TransferInstructionFlags flags, int size, SerializedType? type);
}
