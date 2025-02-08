using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_2;

namespace AssetRipper.Processing;

public abstract class GameObjectHierarchyObject : AssetGroup
{
	public List<IGameObject> GameObjects { get; } = new();
	public List<IComponent> Components { get; } = new();
	public List<IPrefabInstance> PrefabInstances { get; } = new();

	public override IEnumerable<IUnityObjectBase> Assets
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

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		foreach (IUnityObjectBase asset in GameObjects)
		{
			yield return ($"{nameof(GameObjects)}[]", AssetToPPtr(asset));
		}
		foreach (IUnityObjectBase asset in Components)
		{
			yield return ($"{nameof(Components)}[]", AssetToPPtr(asset));
		}
		foreach (IUnityObjectBase asset in PrefabInstances)
		{
			yield return ($"{nameof(PrefabInstances)}[]", AssetToPPtr(asset));
		}
	}

	public sealed override void WalkStandard(AssetWalker walker)
	{
		if (walker.EnterAsset(this))
		{
			WalkFields(walker);
			walker.ExitAsset(this);
		}
	}

	protected virtual void WalkFields(AssetWalker walker)
	{
		this.WalkPPtrListField(walker, GameObjects);

		walker.DivideAsset(this);

		this.WalkPPtrListField(walker, Components);

		walker.DivideAsset(this);

		this.WalkPPtrListField(walker, PrefabInstances);
	}
}
/*
Potential issue:

Any object referenced must be emitted and it is nontrivial to find all the references.
Scene objects can only be referenced within the scene,
but prefab objects can be referenced from anywhere.
*/
