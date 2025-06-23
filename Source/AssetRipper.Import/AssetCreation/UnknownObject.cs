using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Import.AssetCreation;

public sealed class UnknownObject : RawDataObject, INamed
{
	public Utf8String Name
	{
		get => $"Unknown{ClassName}_{RawDataHash:X}";
		set { }
	}

	public UnknownObject(AssetInfo assetInfo, byte[] data) : base(assetInfo, data) { }
}
