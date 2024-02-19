using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_2;

namespace AssetRipper.Processing;

public abstract class GameObjectHierarchyObject : UnityObjectBase
{
	public List<IGameObject> GameObjects { get; } = new();
	public List<IComponent> Components { get; } = new();
	public List<IPrefabInstance> PrefabInstances { get; } = new();

	public virtual IEnumerable<IUnityObjectBase> Assets
	{
		get
		{
			return ((IEnumerable<IUnityObjectBase>)GameObjects)
				.Concat(Components)
				.Concat(PrefabInstances);
		}
	}

	protected GameObjectHierarchyObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}

	public void SetMainAssets()
	{
		MainAsset = this;
		foreach (IUnityObjectBase asset in Assets)
		{
			asset.MainAsset = this;
		}
	}
}
/*
Potential issue:

Any object referenced must be emitted and it is nontrivial to find all the references.
Scene objects can only be referenced within the scene,
but prefab objects can be referenced from anywhere.
*/
