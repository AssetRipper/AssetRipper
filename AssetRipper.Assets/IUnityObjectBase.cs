using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Assets;

public interface IUnityObjectBase : IUnityAssetBase
{
	AssetInfo AssetInfo { get; }
	int ClassID { get; }
	string ClassName { get; }
	AssetCollection Collection { get; }
	long PathID { get; }
}
