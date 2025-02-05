using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_114;

namespace AssetRipper.Processing.Playable;

public sealed class PlayableAssetGroup : UnityObjectBase, INamed
{
	public PlayableAssetGroup(AssetInfo assetInfo, IMonoBehaviour root) : base(assetInfo)
	{
		Root = root;
	}

	public IMonoBehaviour Root { get; set; }
	public List<IMonoBehaviour> Children { get; } = [];

	public IEnumerable<IUnityObjectBase> Assets => Children.Prepend(Root);

	public Utf8String Name { get => Root.Name; set => throw new NotSupportedException(); }

	public void SetMainAssets()
	{
		MainAsset = this;
		foreach (IUnityObjectBase asset in Assets)
		{
			asset.MainAsset = this;
		}
	}

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		yield return (nameof(Root), AssetToPPtr(Root));
		foreach (IUnityObjectBase child in Children)
		{
			yield return ($"{nameof(Children)}[]", AssetToPPtr(child));
		}
	}

	private PPtr AssetToPPtr(IUnityObjectBase? asset) => Collection.ForceCreatePPtr(asset);
}
