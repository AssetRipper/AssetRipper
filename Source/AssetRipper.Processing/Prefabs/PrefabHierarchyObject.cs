using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;

namespace AssetRipper.Processing;

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
	}

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		return base.FetchDependencies().Append((nameof(Prefab), AssetToPPtr(Prefab)));
	}
}
/*
Potential issue:

Any object referenced must be emitted and it is nontrivial to find all the references.
Scene objects can only be referenced within the scene,
but prefab objects can be referenced from anywhere.
*/
