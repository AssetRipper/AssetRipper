using AssetRipper.Assets;

namespace AssetRipper.Tools.DependenceGrapher.Filters;

internal sealed class ClassNameFilter : IAssetFilter
{
	public ClassNameFilter(string name)
	{
		Name = name;
	}

	public string Name { get; }
	public bool IsAcceptable(IUnityObjectBase asset)
	{
		return asset.ClassName == Name;
	}
}
