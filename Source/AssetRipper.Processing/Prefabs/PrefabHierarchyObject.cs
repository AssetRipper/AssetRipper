using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.MarkerInterfaces;

namespace AssetRipper.Processing.Prefabs;

public sealed class PrefabHierarchyObject : GameObjectHierarchyObject, INamed
{
	/// <summary>
	/// The root <see cref="IGameObject"/> of the <see cref="Prefab"/>.
	/// </summary>
	/// <remarks>
	/// This is included in <see cref="GameObjectHierarchyObject.GameObjects"/> because the root GameObject is a part of the hierarchy.
	/// </remarks>
	public IGameObject Root { get; }

	/// <summary>
	/// The <see cref="IPrefabInstance"/> of the <see cref="Root"/>.
	/// </summary>
	/// <remarks>
	/// This is not included in <see cref="GameObjectHierarchyObject.PrefabInstances"/> because it is the source prefab asset, rather than a prefab instance.
	/// </remarks>
	public IPrefabInstance Prefab { get; }

	public override IEnumerable<IUnityObjectBase> Assets => base.Assets.Append(Prefab);

	public Utf8String Name { get => Root.Name; set => throw new NotSupportedException(); }

	public PrefabHierarchyObject(AssetInfo assetInfo, IGameObject root, IPrefabInstance prefab) : base(assetInfo)
	{
		Root = root;
		Prefab = prefab;

		if (Prefab is IPrefabInstanceMarker)
		{
			HiddenAssets.Add(Prefab);
		}
	}

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		return base.FetchDependencies().Append((nameof(Prefab), AssetToPPtr(Prefab)));
	}

	protected override void WalkFields(AssetWalker walker)
	{
		this.WalkPrimitiveField(walker, Name);

		walker.DivideAsset(this);

		base.WalkFields(walker);

		walker.DivideAsset(this);

		this.WalkPPtrField(walker, Root);

		walker.DivideAsset(this);

		this.WalkPPtrField(walker, Prefab);
	}

	public static PrefabHierarchyObject Create(ProcessedAssetCollection collection, IGameObject root, IPrefabInstance prefab)
	{
		PrefabHierarchyObject prefabHierarchy = collection.CreateAsset((int)ClassIDType.PrefabInstance, (assetInfo) => new PrefabHierarchyObject(assetInfo, root, prefab));

		foreach (IEditorExtension asset in root.FetchHierarchy())
		{
			switch (asset)
			{
				case IGameObject gameObject:
					prefabHierarchy.GameObjects.Add(gameObject);
					break;
				case IComponent component:
					prefabHierarchy.Components.Add(component);
					break;
			}
		}

		prefabHierarchy.SetMainAsset();

		return prefabHierarchy;
	}
}
