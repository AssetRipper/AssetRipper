using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_1660057539;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing;

public sealed class SceneHierarchyObject : GameObjectHierarchyObject, INamed
{
	private Utf8String? _name;
	public SceneDefinition Scene { get; }
	public List<ILevelGameManager> Managers { get; } = new();
	public ISceneRoots? SceneRoots { get; set; }

	public override IEnumerable<IUnityObjectBase> Assets => base.Assets.Concat(Managers).MaybeAppend(SceneRoots);

	public Utf8String Name
	{
		get
		{
			if (_name?.String != Scene.Name)
			{
				_name = Scene.Name;
			}
			return _name;
		}
		set => throw new NotSupportedException();
	}

	public SceneHierarchyObject(AssetInfo assetInfo, SceneDefinition scene) : base(assetInfo)
	{
		Scene = scene;
	}

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		foreach ((string, PPtr) pair in base.FetchDependencies())
		{
			yield return pair;
		}
		foreach (IUnityObjectBase asset in Managers)
		{
			yield return ($"{nameof(Managers)}[]", AssetToPPtr(asset));
		}
		yield return (nameof(SceneRoots), AssetToPPtr(SceneRoots));
	}
}
/*
Potential issue:

Any object referenced must be emitted and it is nontrivial to find all the references.
Scene objects can only be referenced within the scene,
but prefab objects can be referenced from anywhere.
*/
