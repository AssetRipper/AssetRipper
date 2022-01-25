using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
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
		/// <summary>
		/// Release or less than 2.1.0
		/// </summary>
		public static bool HasTag(UnityVersion version, TransferInstructionFlags flags) => flags.IsRelease() || version.IsLess(2, 1);
		/// <summary>
		/// 2.1.0 and greater and Not Release
		/// </summary>
		public static bool HasTagString(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(2, 1) && !flags.IsRelease();
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool IsActiveInherited(UnityVersion version) => version.IsLess(4);

		public static bool GetIsActive(this IGameObject gameObject)
		{
			if (IsActiveInherited(gameObject.AssetUnityVersion))
			{
				return gameObject.SerializedFile.Collection.IsScene(gameObject.SerializedFile) ? gameObject.IsActive : true;
			}
			return gameObject.IsActive;
		}

		public static ushort GetTag(this IGameObject gameObject, IExportContainer container)
		{
			if (HasTag(gameObject.AssetUnityVersion, gameObject.TransferInstructionFlags))
			{
				return gameObject.Tag;
			}
			return container.TagNameToID(gameObject.TagString);
		}

		public static string GetTagString(this IGameObject gameObject, IExportContainer container)
		{
			if (HasTagString(gameObject.AssetUnityVersion, gameObject.TransferInstructionFlags))
			{
				return gameObject.TagString;
			}
			return container.TagIDToName(gameObject.Tag);
		}

		public static T FindComponent<T>(this IGameObject gameObject) where T : IComponent
		{
			foreach (PPtr<IComponent> ptr in gameObject.FetchComponents())
			{
				// component could have not implemented asset type
				IComponent comp = ptr.FindAsset(gameObject.SerializedFile);
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
				IComponent comp = ptr.FindAsset(gameObject.SerializedFile);
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
				ITransform parent = root.FatherPtr.TryGetAsset(root.SerializedFile);
				if (parent == null)
				{
					break;
				}
				else
				{
					root = parent;
				}
			}
			return root.GameObjectPtr.GetAsset(root.SerializedFile);
		}

		public static int GetRootDepth(this IGameObject gameObject)
		{
			ITransform root = gameObject.GetTransform();
			int depth = 0;
			while (true)
			{
				ITransform parent = root.FatherPtr.TryGetAsset(root.SerializedFile);
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
				IComponent component = ptr.FindAsset(root.SerializedFile);
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
				ITransform child = pchild.GetAsset(transform.SerializedFile);
				IGameObject childGO = child.GameObjectPtr.GetAsset(root.SerializedFile);
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
				ITransform childTransform = childPtr.GetAsset(gameObject.SerializedFile);
				IGameObject child = childTransform.GameObjectPtr.GetAsset(gameObject.SerializedFile);
				string path = string.IsNullOrEmpty(parentPath) ? child.Name : $"{parentPath}/{child.Name}";
				uint pathHash = CrcUtils.CalculateDigestUTF8(path);
				tos[pathHash] = path;

				gameObject.BuildTOS(child, path, tos);
			}
		}
	}
}
