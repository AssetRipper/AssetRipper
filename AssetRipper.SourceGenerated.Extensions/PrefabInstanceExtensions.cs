using AssetRipper.Assets.Interfaces;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_18;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class PrefabInstanceExtensions
	{
		public static string GetName(this IPrefabInstance prefab)
		{
			string? name;
			if (prefab is IHasNameString hasName)
			{
				name = hasName.NameString;
			}
			else
			{
				name = prefab.RootGameObject_C1001P?.NameString;
			}
			return string.IsNullOrEmpty(name) ? prefab.ClassName : name;
		}

		public static IEnumerable<IEditorExtension> GetObjects(this IPrefabInstance prefab)
		{
			if (prefab.Has_Objects_C1001())
			{
				return prefab.Objects_C1001P;
			}
			else
			{
				return prefab.RootGameObject_C1001P?.FetchHierarchy() ?? Enumerable.Empty<IEditorExtension>();
			}
		}

		public static IGameObject? TryGetRootGameObject(this IPrefabInstance prefab)
		{
			if (prefab.Has_RootGameObject_C1001())
			{
				return prefab.RootGameObject_C1001P;
			}
			else
			{
				return prefab.Objects_C1001P.OfType<IGameObject>().FirstOrDefault()?.GetRoot();
			}
		}

		public static IGameObject GetRootGameObject(this IPrefabInstance prefab)
		{
			return prefab.TryGetRootGameObject() ?? throw new ArgumentException("Prefab has no root GameObject.", nameof(prefab));
		}
	}
}
