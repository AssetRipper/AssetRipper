using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Project;
using AssetRipper.Core.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Component_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Transform_;
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
		public static bool IsActiveInherited(UnityVersion version) => version.IsLess(4);

		private static bool IsActive(this IGameObject gameObject) => gameObject.IsActive_C1_Boolean || gameObject.IsActive_C1_Byte > 0;

		private static bool GetIsActive(this IGameObject gameObject)
		{
			if (IsActiveInherited(gameObject.SerializedFile.Version))
			{
				return gameObject.SerializedFile.Collection.IsScene(gameObject.SerializedFile) ? gameObject.IsActive() : true;
			}
			return gameObject.IsActive();
		}

		public static void ConvertToEditorFormat(this IGameObject gameObject, IExportContainer container)
		{
			bool isActive = gameObject.GetIsActive();
			gameObject.IsActive_C1_Byte = isActive ? (byte)1 : (byte)0;
			gameObject.IsActive_C1_Boolean = isActive;
			gameObject.TagString_C1.String = container.TagIDToName(gameObject.Tag_C1);
		}

		public static IEnumerable<IPPtr_Component_> FetchComponents(this IGameObject gameObject)
		{
			if (gameObject.Component_C1_AssetList_ComponentPair is not null)
			{
				return gameObject.Component_C1_AssetList_ComponentPair.Select(pair => pair.m_Component);
			}
			else if (gameObject.Component_C1_AssetList_NullableKeyValuePair_Int32_PPtr_Component__3_0_0_f5 is not null)
			{
				return gameObject.Component_C1_AssetList_NullableKeyValuePair_Int32_PPtr_Component__3_0_0_f5.Select(pair => pair.Value);
			}
			else if (gameObject.Component_C1_AssetList_NullableKeyValuePair_Int32_PPtr_Component__5_0_0_f4 is not null)
			{
				return gameObject.Component_C1_AssetList_NullableKeyValuePair_Int32_PPtr_Component__5_0_0_f4.Select(pair => pair.Value);
			}
			else
			{
				throw new Exception("All three component properties returned null");
			}
		}

		public static T? TryGetComponent<T>(this IGameObject gameObject) where T : IComponent
		{
			foreach (IPPtr_Component_ ptr in gameObject.FetchComponents())
			{
				// component could have not implemented asset type
				IComponent? comp = ptr.TryGetAsset(gameObject.SerializedFile);
				if (comp is T t)
				{
					return t;
				}
			}
			return default;
		}
		
		public static bool TryGetComponent<T>(this IGameObject gameObject, [NotNullWhen(true)] out T? component) where T : IComponent
		{
			component = gameObject.TryGetComponent<T>();
			return component is not null;
		}

		public static T GetComponent<T>(this IGameObject gameObject) where T : IComponent
		{
			T? component = gameObject.TryGetComponent<T>();
			if (component is null)
			{
				throw new Exception($"Component of type {typeof(T)} hasn't been found");
			}
			return component;
		}
		
		public static ITransform GetTransform(this IGameObject gameObject)
		{
			foreach (IPPtr_Component_ ptr in gameObject.FetchComponents())
			{
				IComponent? comp = ptr.TryGetAsset(gameObject.SerializedFile);
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
				ITransform? parent = root.Father_C4.TryGetAsset(root.SerializedFile);
				if (parent == null)
				{
					break;
				}
				else
				{
					root = parent;
				}
			}
			return root.GameObject_C4.GetAsset(root.SerializedFile);
		}

		public static int GetRootDepth(this IGameObject gameObject)
		{
			ITransform root = gameObject.GetTransform();
			int depth = 0;
			while (true)
			{
				ITransform? parent = root.Father_C4.TryGetAsset(root.SerializedFile);
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
			foreach (IPPtr_Component_ ptr in root.FetchComponents())
			{
				IComponent? component = ptr.TryGetAsset(root.SerializedFile);
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

			foreach (IPPtr_Transform_ pchild in transform.Children_C4)
			{
				ITransform child = pchild.GetAsset(transform.SerializedFile);
				IGameObject childGO = child.GameObject_C4.GetAsset(child.SerializedFile);
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
			foreach (IPPtr_Transform_ childPtr in transform.Children_C4)
			{
				ITransform childTransform = childPtr.GetAsset(gameObject.SerializedFile);
				IGameObject child = childTransform.GameObject_C4.GetAsset(gameObject.SerializedFile);
				string path = string.IsNullOrEmpty(parentPath) ? child.NameString : $"{parentPath}/{child.NameString}";
				uint pathHash = CrcUtils.CalculateDigestUTF8(path);
				tos[pathHash] = path;

				gameObject.BuildTOS(child, path, tos);
			}
		}
	}
}
