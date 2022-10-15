using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Utils;
using AssetRipper.Core.Extensions;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_78;
using AssetRipper.SourceGenerated.Subclasses.ComponentPair;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Component_;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class GameObjectExtensions
	{
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		private static bool IsActiveInherited(UnityVersion version) => version.IsLess(4);

		public static bool GetIsActive(this IGameObject gameObject)
		{
			return gameObject.IsActive_C1_Boolean || gameObject.IsActive_C1_Byte > 0;
		}

		public static void SetIsActive(this IGameObject gameObject, bool active)
		{
			gameObject.IsActive_C1_Byte = active ? (byte)1 : (byte)0;
			gameObject.IsActive_C1_Boolean = active;
		}

		private static bool ShouldBeActive(this IGameObject gameObject)
		{
			if (IsActiveInherited(gameObject.Collection.Version))
			{
				return gameObject.Collection.IsScene() ? gameObject.GetIsActive() : true;
			}
			return gameObject.GetIsActive();
		}

		public static void ConvertToEditorFormat(this IGameObject gameObject, ITagManager? tagManager)
		{
			gameObject.SetIsActive(gameObject.ShouldBeActive());
			gameObject.TagString_C1.String = tagManager.TagIDToName(gameObject.Tag_C1);
		}

		public static IEnumerable<IPPtr_Component_> FetchComponents(this IGameObject gameObject)
		{
			if (gameObject.Has_Component_C1_AssetList_ComponentPair())
			{
				return gameObject.Component_C1_AssetList_ComponentPair.Select(pair => pair.m_Component);
			}
			else if (gameObject.Has_Component_C1_AssetList_AssetPair_Int32_PPtr_Component__3_0_0_f5())
			{
				return gameObject.Component_C1_AssetList_AssetPair_Int32_PPtr_Component__3_0_0_f5.Select(pair => pair.Value);
			}
			else if (gameObject.Has_Component_C1_AssetList_AssetPair_Int32_PPtr_Component__5_0_0_f4())
			{
				return gameObject.Component_C1_AssetList_AssetPair_Int32_PPtr_Component__5_0_0_f4.Select(pair => pair.Value);
			}
			else
			{
				throw new Exception("All three component properties returned null");
			}
		}

		public static AccessListBase<IPPtr_Component_> GetComponentPPtrList(this IGameObject gameObject)
		{
			if (gameObject.Has_Component_C1_AssetList_ComponentPair())
			{
				return new ComponentPairAccessList(gameObject.Component_C1_AssetList_ComponentPair);
			}
			else if (gameObject.Has_Component_C1_AssetList_AssetPair_Int32_PPtr_Component__3_0_0_f5())
			{
				return new AssetPairAccessList<PPtr_Component__3_0_0_f5>(gameObject.Component_C1_AssetList_AssetPair_Int32_PPtr_Component__3_0_0_f5);
			}
			else if (gameObject.Has_Component_C1_AssetList_AssetPair_Int32_PPtr_Component__5_0_0_f4())
			{
				return new AssetPairAccessList<PPtr_Component__5_0_0_f4>(gameObject.Component_C1_AssetList_AssetPair_Int32_PPtr_Component__5_0_0_f4);
			}
			else
			{
				throw new Exception("All three component properties returned null");
			}
		}

		public static void AddComponent(this IGameObject gameObject, ClassIDType classID, IComponent component)
		{
			if (gameObject.Has_Component_C1_AssetList_ComponentPair())
			{
				gameObject.Component_C1_AssetList_ComponentPair.AddNew().Component.SetAsset(gameObject.Collection, component);
			}
			else if (gameObject.Has_Component_C1_AssetList_AssetPair_Int32_PPtr_Component__3_0_0_f5())
			{
				AssetPair<int, PPtr_Component__3_0_0_f5> pair = gameObject.Component_C1_AssetList_AssetPair_Int32_PPtr_Component__3_0_0_f5.AddNew();
				pair.Key = (int)classID;
				pair.Value.SetAsset(gameObject.Collection, component);
			}
			else if (gameObject.Has_Component_C1_AssetList_AssetPair_Int32_PPtr_Component__5_0_0_f4())
			{
				AssetPair<int, PPtr_Component__5_0_0_f4> pair = gameObject.Component_C1_AssetList_AssetPair_Int32_PPtr_Component__5_0_0_f4.AddNew();
				pair.Key = (int)classID;
				pair.Value.SetAsset(gameObject.Collection, component);
			}
			else
			{
				throw new Exception("All three component properties returned null");
			}
		}

		public static PPtrAccessList<IPPtr_Component_, IComponent> GetComponentAccessList(this IGameObject gameObject)
		{
			return new PPtrAccessList<IPPtr_Component_, IComponent>(gameObject.GetComponentPPtrList(), gameObject.Collection);
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
			ITransform root = gameObject.GetTransform();
			while (true)
			{
				ITransform? parent = root.Father_C4P;
				if (parent == null)
				{
					break;
				}
				else
				{
					root = parent;
				}
			}
			return root.GameObject_C4.GetAsset(root.Collection);
		}

		public static int GetRootDepth(this IGameObject gameObject)
		{
			ITransform root = gameObject.GetTransform();
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

		public static IEnumerable<IEditorExtension> FetchHierarchy(this IGameObject root)
		{
			yield return root;

			ITransform? transform = null;
			foreach (IComponent? component in root.GetComponentAccessList())
			{
				if (component == null)
				{
					continue;
				}

				yield return component;
				if (component is ITransform trfm)
				{
					transform = trfm;
				}
			}

			if (transform is null)
			{
				throw new Exception("GameObject has no transform");
			}

			foreach (ITransform? child in transform.Children_C4P)
			{
				_ = child ?? throw new NullReferenceException();
				IGameObject childGO = child.GameObject_C4P ?? throw new NullReferenceException();
				foreach (IEditorExtension childElement in FetchHierarchy(childGO))
				{
					yield return childElement;
				}
			}
		}

		public static IReadOnlyDictionary<uint, string> BuildTOS(this IGameObject gameObject)
		{
			Dictionary<uint, string> tos = new() { { 0, string.Empty } };
			gameObject.BuildTOS(gameObject, string.Empty, tos);
			return tos;
		}

		private static void BuildTOS(this IGameObject gameObject, IGameObject parent, string parentPath, Dictionary<uint, string> tos)
		{
			ITransform transform = parent.GetTransform();
			foreach (ITransform? childTransform in transform.Children_C4P)
			{
				_ = childTransform ?? throw new NullReferenceException();
				IGameObject child = childTransform.GameObject_C4P ?? throw new NullReferenceException();
				string path = string.IsNullOrEmpty(parentPath) ? child.NameString : $"{parentPath}/{child.NameString}";
				uint pathHash = CrcUtils.CalculateDigestUTF8(path);
				tos[pathHash] = path;

				gameObject.BuildTOS(child, path, tos);
			}
		}

		private sealed class ComponentPairAccessList : AccessListBase<IPPtr_Component_>
		{
			private readonly AssetList<ComponentPair> referenceList;

			public ComponentPairAccessList(AssetList<ComponentPair> referenceList)
			{
				this.referenceList = referenceList;
			}

			public override IPPtr_Component_ this[int index] { get => referenceList[index].Component; set => referenceList[index].Component.CopyValues(value); }

			public override int Count => referenceList.Count;

			public override int Capacity { get => referenceList.Capacity; set => referenceList.Capacity = value; }

			public override void Add(IPPtr_Component_ item)
			{
				ComponentPair pair = Convert(item);
				referenceList.Add(pair);
			}

			private static ComponentPair Convert(IPPtr_Component_ item)
			{
				ComponentPair pair = new();
				pair.Component.CopyValues(item);
				return pair;
			}

			public override IPPtr_Component_ AddNew()
			{
				return referenceList.AddNew().Component;
			}

			public override void Clear()
			{
				referenceList.Clear();
			}

			public override bool Contains(IPPtr_Component_ item)
			{
				return referenceList.Contains(Convert(item));
			}

			public override void CopyTo(IPPtr_Component_[] array, int arrayIndex)
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

			public override int IndexOf(IPPtr_Component_ item)
			{
				return referenceList.IndexOf(Convert(item));
			}

			public override void Insert(int index, IPPtr_Component_ item)
			{
				referenceList.Insert(index, Convert(item));
			}

			public override bool Remove(IPPtr_Component_ item)
			{
				return referenceList.Remove(Convert(item));
			}

			public override void RemoveAt(int index)
			{
				referenceList.RemoveAt(index);
			}
		}

		private sealed class AssetPairAccessList<T> : AccessListBase<IPPtr_Component_> where T : IPPtr_Component_, new()
		{
			private readonly AssetList<AssetPair<int, T>> referenceList;

			public AssetPairAccessList(AssetList<AssetPair<int, T>> referenceList)
			{
				this.referenceList = referenceList;
			}

			public override IPPtr_Component_ this[int index]
			{
				get => referenceList[index].Value;
				set => referenceList[index].Value.CopyValues(value);
			}

			public override int Count => referenceList.Count;

			public override int Capacity { get => referenceList.Capacity; set => referenceList.Capacity = value; }

			public override void Add(IPPtr_Component_ item)
			{
				//referenceList.Add(CreateNewPair(item));
				throw new NotSupportedException();
			}

			private static AssetPair<int, T> CreateNewPair(IPPtr_Component_ item)
			{
				AssetPair<int, T> pair = new();
				pair.Key = 2;
				pair.Value.CopyValues(item);
				return pair;
			}

			public override IPPtr_Component_ AddNew()
			{
				//AssetPair<int, T> pair = referenceList.AddNew();
				//pair.Key = 2;
				//return pair.Value;
				throw new NotSupportedException();
				//Not sure the above code is safe since Unity might rely on the class id being correct.
			}

			public override void Clear()
			{
				referenceList.Clear();
			}

			public override bool Contains(IPPtr_Component_ item)
			{
				return referenceList.Any(ptr => ptr.Value.Equals(item));
			}

			public override void CopyTo(IPPtr_Component_[] array, int arrayIndex)
			{
				for (int i = 0; i < referenceList.Count; i++)
				{
					array[i + arrayIndex] = referenceList[i].Value;
				}
			}

			public override int EnsureCapacity(int capacity)
			{
				return referenceList.EnsureCapacity(capacity);
			}

			public override int IndexOf(IPPtr_Component_ item)
			{
				return referenceList.IndexOf(pair => pair.Value.Equals(item));
			}

			public override void Insert(int index, IPPtr_Component_ item)
			{
				//referenceList.Insert(index, CreateNewPair(item));
				throw new NotSupportedException();
			}

			public override bool Remove(IPPtr_Component_ item)
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
}
