using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_1660057539;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing;

public sealed class SceneHierarchyObject : GameObjectHierarchyObject
{
	public SceneDefinition Scene { get; }
	public List<ILevelGameManager> Managers { get; } = new();
	public ISceneRoots? SceneRoots { get; set; }

	public override IEnumerable<IUnityObjectBase> Assets => base.Assets.Concat(Managers).MaybeAppend(SceneRoots);

	public SceneHierarchyObject(AssetInfo assetInfo, SceneDefinition scene) : base(assetInfo)
	{
		Scene = scene;
	}
}
/*
Potential issue:

Any object referenced must be emitted and it is nontrivial to find all the references.
Scene objects can only be referenced within the scene,
but prefab objects can be referenced from anywhere.
*/
