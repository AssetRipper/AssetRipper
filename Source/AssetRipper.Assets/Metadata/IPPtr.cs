using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets.Metadata;

public interface IPPtr : IUnityAssetBase
{
	/// <summary>
	/// Zero means the asset is located within the current file.
	/// </summary>
	int FileID { get; }
	/// <summary>
	/// It is sometimes sequential and sometimes more like a hash. Zero signifies a null reference.
	/// </summary>
	long PathID { get; }
}

public interface IPPtr<T> : IPPtr where T : IUnityObjectBase
{
	void SetAsset(AssetCollection collection, T? asset);
	bool TryGetAsset(AssetCollection collection, [NotNullWhen(true)] out T? asset);
}
