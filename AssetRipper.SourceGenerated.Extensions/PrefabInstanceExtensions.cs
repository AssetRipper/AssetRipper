using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Subclasses.PPtr_EditorExtension;

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

		public static IEnumerable<IEditorExtension> FetchObjects(this IPrefabInstance prefab)
		{
			if (prefab.Has_RootGameObject_C1001())
			{
				foreach (IEditorExtension asset in prefab.RootGameObject_C1001.GetAsset(prefab.Collection).FetchHierarchy())
				{
					yield return asset;
				}
			}
			else if (prefab.Has_Objects_C1001())//DataTemplate
			{
				foreach (PPtr_EditorExtension_3_4_0 asset in prefab.Objects_C1001)
				{
					yield return asset.GetAsset(prefab.Collection);
				}
			}
		}
	}
}
