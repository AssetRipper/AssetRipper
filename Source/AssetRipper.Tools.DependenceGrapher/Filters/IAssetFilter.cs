using AssetRipper.Assets;

namespace AssetRipper.Tools.DependenceGrapher.Filters;

internal interface IAssetFilter
{
	bool IsAcceptable(IUnityObjectBase asset);
}
