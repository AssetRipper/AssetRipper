using AssetRipper.Assets;

namespace AssetRipper.Tools.DependenceGrapher.Filters;

internal sealed class NameFilter : IAssetFilter
{
	public NameFilter(string name)
	{
		Name = name;
	}

	public string Name { get; }
	public bool IsAcceptable(IUnityObjectBase asset)
	{
		return asset.GetName() == Name;
	}
}
