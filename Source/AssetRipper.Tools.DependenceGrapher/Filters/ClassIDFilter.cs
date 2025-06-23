using AssetRipper.Assets;

namespace AssetRipper.Tools.DependenceGrapher.Filters;

internal sealed class ClassIDFilter : IAssetFilter
{
	public ClassIDFilter(int classID)
	{
		ClassID = classID;
	}

	public int ClassID { get; }
	public bool IsAcceptable(IUnityObjectBase asset)
	{
		return asset.ClassID == ClassID;
	}
}
