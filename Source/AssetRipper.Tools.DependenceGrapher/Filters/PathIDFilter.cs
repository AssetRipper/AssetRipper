using AssetRipper.Assets;

namespace AssetRipper.Tools.DependenceGrapher.Filters;

internal sealed class PathIDFilter : IAssetFilter
{
	public PathIDFilter(long pathID)
	{
		PathID = pathID;
	}

	public long PathID { get; }
	public bool IsAcceptable(IUnityObjectBase asset)
	{
		return asset.PathID == PathID;
	}
}
