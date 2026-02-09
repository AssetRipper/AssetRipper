using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_1660057539;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing.Prefabs;

public sealed class SceneHierarchyObject : GameObjectHierarchyObject, INamed
{
	private Utf8String? _name;
	public SceneDefinition Scene { get; }
	public List<ILevelGameManager> Managers { get; } = [];
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

	protected override void WalkFields(AssetWalker walker)
	{
		this.WalkPrimitiveField(walker, Name);

		walker.DivideAsset(this);

		base.WalkFields(walker);

		walker.DivideAsset(this);

		this.WalkPPtrListField(walker, Managers);

		walker.DivideAsset(this);

		this.WalkPPtrField(walker, SceneRoots);
	}

	public IEnumerable<IGameObject> GetRoots()
	{
		return GameObjects.Where(GameObjectExtensions.IsRoot);
	}

	public static SceneHierarchyObject Create(ProcessedAssetCollection collection, SceneDefinition scene)
	{
		SceneHierarchyObject sceneHierarchy = collection.CreateAsset((int)ClassIDType.SceneAsset, (assetInfo) => new SceneHierarchyObject(assetInfo, scene));

		foreach (IUnityObjectBase asset in scene.Assets)
		{
			switch (asset)
			{
				case IGameObject gameObject:
					sceneHierarchy.GameObjects.Add(gameObject);
					break;
				case IMonoBehaviour monoBehaviour:
					if (monoBehaviour.IsComponentOnGameObject())
					{
						sceneHierarchy.Components.Add(monoBehaviour);
					}
					break;
				case IComponent component:
					sceneHierarchy.Components.Add(component);
					break;
				case ILevelGameManager manager:
					sceneHierarchy.Managers.Add(manager);
					break;
				case IPrefabInstance prefabInstance:
					sceneHierarchy.PrefabInstances.Add(prefabInstance);
					break;
				case ISceneRoots sceneRoots:
					sceneHierarchy.SceneRoots = sceneRoots;
					break;
			}
		}

		sceneHierarchy.SetMainAsset();

		return sceneHierarchy;
	}
}
