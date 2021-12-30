using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.GameObject
{
	public interface IGameObject : IEditorExtension, IHasName
	{
		ushort Tag { get; set; }
		string TagString { get; set; }
		bool IsActive { get; set; }
		uint Layer { get; set; }
		PPtr<IComponent>[] FetchComponents();
	}

	public static class GameObjectExtensions
	{
		public static T FindComponent<T>(this IGameObject gameObject) where T : IComponent
		{
			foreach (PPtr<IComponent> ptr in gameObject.FetchComponents())
			{
				// component could have not implemented asset type
				IComponent comp = ptr.FindAsset(gameObject.File);
				if (comp is T t)
				{
					return t;
				}
			}
			return default;
		}

		public static T GetComponent<T>(this IGameObject gameObject) where T : IComponent
		{
			T component = gameObject.FindComponent<T>();
			if (component == null)
			{
				throw new Exception($"Component of type {nameof(T)} hasn't been found");
			}
			return component;
		}

		public static ITransform GetTransform(this IGameObject gameObject)
		{
			foreach (PPtr<IComponent> ptr in gameObject.FetchComponents())
			{
				IComponent comp = ptr.FindAsset(gameObject.File);
				if (comp == null)
				{
					continue;
				}

				if (comp is ITransform transform)
				{
					return transform;
				}
			}
			throw new Exception("Can't find transform component");
		}

		public static IGameObject GetRoot(this IGameObject gameObject)
		{
			ITransform root = gameObject.GetTransform();
			while (true)
			{
				ITransform parent = root.FatherPtr.TryGetAsset(root.File);
				if (parent == null)
				{
					break;
				}
				else
				{
					root = parent;
				}
			}
			return root.GameObjectPtr.GetAsset(root.File);
		}

		public static int GetRootDepth(this IGameObject gameObject)
		{
			ITransform root = gameObject.GetTransform();
			int depth = 0;
			while (true)
			{
				ITransform parent = root.FatherPtr.TryGetAsset(root.File);
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

			ITransform transform = null;
			foreach (PPtr<IComponent> ptr in root.FetchComponents())
			{
				IComponent component = ptr.FindAsset(root.File);
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

			foreach (PPtr<ITransform> pchild in transform.ChildrenPtrs)
			{
				ITransform child = pchild.GetAsset(transform.File);
				IGameObject childGO = child.GameObjectPtr.GetAsset(root.File);
				foreach (IEditorExtension childElement in FetchHierarchy(childGO))
				{
					yield return childElement;
				}
			}
		}

		public static IReadOnlyDictionary<uint, string> BuildTOS(this IGameObject gameObject)
		{
			Dictionary<uint, string> tos = new Dictionary<uint, string>() { { 0, string.Empty } };
			gameObject.BuildTOS(gameObject, string.Empty, tos);
			return tos;
		}

		private static void BuildTOS(this IGameObject gameObject, IGameObject parent, string parentPath, Dictionary<uint, string> tos)
		{
			ITransform transform = parent.GetTransform();
			foreach (PPtr<ITransform> childPtr in transform.ChildrenPtrs)
			{
				ITransform childTransform = childPtr.GetAsset(gameObject.File);
				IGameObject child = childTransform.GameObjectPtr.GetAsset(gameObject.File);
				string path = string.IsNullOrEmpty(parentPath) ? child.Name : $"{parentPath}/{child.Name}";
				uint pathHash = CrcUtils.CalculateDigestUTF8(path);
				tos[pathHash] = path;

				gameObject.BuildTOS(child, path, tos);
			}
		}
	}
}
