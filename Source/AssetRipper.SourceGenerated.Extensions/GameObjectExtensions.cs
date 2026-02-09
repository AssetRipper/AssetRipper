using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_78;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.ComponentPair;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Component;

namespace AssetRipper.SourceGenerated.Extensions;

public static class GameObjectExtensions
{
	/// <summary>
	/// Less than 4.0.0
	/// </summary>
	private static bool IsActiveInherited(UnityVersion version) => version.LessThan(4);

	public static bool GetIsActive(this IGameObject gameObject)
	{
		return gameObject.IsActive_Boolean || gameObject.IsActive_Byte > 0;
	}

	public static void SetIsActive(this IGameObject gameObject, bool active)
	{
		gameObject.IsActive_Byte = active ? (byte)1 : (byte)0;
		gameObject.IsActive_Boolean = active;
	}

	private static bool ShouldBeActive(this IGameObject gameObject)
	{
		if (IsActiveInherited(gameObject.Collection.Version) && !gameObject.Collection.IsScene)
		{
			return true;
		}
		return gameObject.GetIsActive();
	}

	public static void ConvertToEditorFormat(this IGameObject gameObject, ITagManager? tagManager)
	{
		gameObject.SetIsActive(gameObject.ShouldBeActive());
		gameObject.TagString = tagManager.TagIDToName(gameObject.Tag);
	}

	public static IEnumerable<IPPtr_Component> FetchComponents(this IGameObject gameObject)
	{
		return gameObject.Components.Select(pair => pair.Component);
	}

	public static AccessListBase<IPPtr_Component> GetComponentPPtrList(this IGameObject gameObject)
	{
		return new ComponentPairAccessList(gameObject.Components);
	}

	public static int GetComponentCount(this IGameObject gameObject)
	{
		return gameObject.Components.Count;
	}

	public static void AddComponent(this IGameObject gameObject, ClassIDType classID, IComponent component)
	{
		IComponentPair pair = gameObject.Components.AddNew();
		pair.ClassID = (int)classID;
		pair.Component.SetAsset(gameObject.Collection, component);
	}

	public static PPtrAccessList<IPPtr_Component, IComponent> GetComponentAccessList(this IGameObject gameObject)
	{
		return new PPtrAccessList<IPPtr_Component, IComponent>(gameObject.GetComponentPPtrList(), gameObject.Collection);
	}

	public static T? TryGetComponent<T>(this IGameObject gameObject) where T : IComponent
	{
		gameObject.TryGetComponent(out T? component);
		return component;
	}

	public static bool TryGetComponent<T>(this IGameObject gameObject, [NotNullWhen(true)] out T? component) where T : IComponent
	{
		foreach (IComponent? comp in gameObject.GetComponentAccessList())
		{
			// component could have not implemented asset type
			if (comp is T t)
			{
				component = t;
				return true;
			}
		}
		component = default;
		return false;
	}

	public static T GetComponent<T>(this IGameObject gameObject) where T : IComponent
	{
		if (!gameObject.TryGetComponent(out T? component))
		{
			throw new Exception($"Component of type {typeof(T)} hasn't been found");
		}
		return component;
	}

	public static ITransform GetTransform(this IGameObject gameObject)
	{
		foreach (IComponent? component in gameObject.GetComponentAccessList())
		{
			if (component is ITransform transform)
			{
				return transform;
			}
		}
		throw new Exception("Can't find transform component");
	}

	public static bool IsRoot(this IGameObject gameObject)
	{
		return gameObject.TryGetComponent<ITransform>()?.Father_C4P is null;
	}

	public static IGameObject GetRoot(this IGameObject gameObject)
	{
		if (!gameObject.TryGetComponent(out ITransform? root))
		{
			return gameObject;
		}

		while (true)
		{
			ITransform? parent = root.Father_C4P;
			if (parent is null || parent.GameObject_C4P is null)
			{
				break;
			}
			else
			{
				root = parent;
			}
		}
		return root.GameObject_C4P!;
	}

	public static int GetRootDepth(this IGameObject gameObject)
	{
		if (!gameObject.TryGetComponent(out ITransform? root))
		{
			return 0;
		}

		int depth = 0;
		while (true)
		{
			ITransform? parent = root.Father_C4P;
			if (parent == null)
			{
				break;
			}

			root = parent;
			depth++;
		}
		return depth;
	}

	/// <summary>
	/// Fetch all the assets in the hierarchy for this <see cref="IGameObject"/>.
	/// </summary>
	/// <remarks>
	/// This includes the <paramref name="root"/>.
	/// </remarks>
	/// <param name="root"></param>
	/// <returns></returns>
	/// <exception cref="NullReferenceException">A referenced asset wasn't found.</exception>
	public static IEnumerable<IEditorExtension> FetchHierarchy(this IGameObject root)
	{
		yield return root;

		ITransform? transform = null;
		foreach (IComponent component in root.GetComponentAccessList().WhereNotNull())
		{
			yield return component;

			if (component is ITransform trfm)
			{
				transform = trfm;
			}
		}

		if (transform is null)
		{
			yield break;
		}

		foreach (IGameObject child in transform.Children_C4P.WhereNotNull().Select(t => t.GameObject_C4P).WhereNotNull())
		{
			foreach (IEditorExtension childElement in child.FetchHierarchy())
			{
				yield return childElement;
			}
		}
	}

	public static IEnumerable<IGameObject> GetChildren(this IGameObject gameObject)
	{
		ITransform transform = gameObject.GetTransform();
		return transform.Children_C4P.WhereNotNull().Select(t => t.GameObject_C4P).WhereNotNull();
	}

	public static IPrefabInstance CreatePrefabForRoot(this IGameObject root, ProcessedAssetCollection collection)
	{
		IPrefabInstance prefab = collection.CreatePrefabInstance();

		prefab.HideFlagsE = HideFlags.HideInHierarchy;
		prefab.RootGameObjectP = root;
		prefab.IsPrefabAsset = true;
		prefab.AssetBundleName = root.AssetBundleName;

		prefab.OriginalDirectory = root.OriginalDirectory;
		prefab.OriginalName = root.OriginalName;
		prefab.OriginalExtension = root.OriginalExtension;

		prefab.OverrideDirectory = root.GetBestDirectory();
		prefab.OverrideName = root.GetBestName();
		prefab.OverrideExtension = root.GetBestExtension();

		prefab.SetPrefabInternal();

		return prefab;
	}

	private sealed class ComponentPairAccessList : AccessListBase<IPPtr_Component>
	{
		private readonly AccessListBase<IComponentPair> referenceList;

		public ComponentPairAccessList(AccessListBase<IComponentPair> referenceList)
		{
			this.referenceList = referenceList;
		}

		public override IPPtr_Component this[int index]
		{
			get => referenceList[index].Component;
			set => throw new NotSupportedException();
		}

		public override int Count => referenceList.Count;

		public override int Capacity { get => referenceList.Capacity; set => referenceList.Capacity = value; }

		public override void Add(IPPtr_Component item)
		{
			throw new NotSupportedException();
		}

		public override IPPtr_Component AddNew()
		{
			IComponentPair componentPair = referenceList.AddNew();
			componentPair.ClassID = (int)ClassIDType.Component;
			return componentPair.Component;
			//throw new NotSupportedException();
			//Not sure the above code is safe since Unity might rely on the class id being correct.
		}

		public override void Clear()
		{
			referenceList.Clear();
		}

		public override bool Contains(IPPtr_Component item)
		{
			return referenceList.Any(ptr => ptr.Component.Equals(item));
		}

		public override void CopyTo(IPPtr_Component[] array, int arrayIndex)
		{
			for (int i = 0; i < referenceList.Count; i++)
			{
				array[i + arrayIndex] = referenceList[i].Component;
			}
		}

		public override int EnsureCapacity(int capacity)
		{
			return referenceList.EnsureCapacity(capacity);
		}

		public override int IndexOf(IPPtr_Component item)
		{
			return referenceList.IndexOf(pair => pair.Component.Equals(item));
		}

		public override void Insert(int index, IPPtr_Component item)
		{
			throw new NotSupportedException();
		}

		public override bool Remove(IPPtr_Component item)
		{
			int index = IndexOf(item);
			if (index < 0)
			{
				return false;
			}
			else
			{
				RemoveAt(index);
				return true;
			}
		}

		public override void RemoveAt(int index)
		{
			referenceList.RemoveAt(index);
		}
	}
}
