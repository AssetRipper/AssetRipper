using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Subclasses.PPtr_EditorExtension_;
using System.Collections.Generic;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class PrefabInstanceExtensions
	{
		public static string GetName(this IPrefabInstance prefab, IAssetContainer rootGameObjectFile)
		{
			if (prefab is IHasNameString hasName && string.IsNullOrEmpty(hasName.NameString))
			{
				return hasName.NameString;
			}
			else
			{
				return prefab.RootGameObject_C1001?.TryGetAsset(rootGameObjectFile)?.NameString ?? prefab.AssetClassName;
			}
		}

		public static IEnumerable<IEditorExtension> FetchObjects(this IPrefabInstance prefab, IAssetContainer file)
		{
			if (prefab.Has_RootGameObject_C1001())
			{
				foreach (IEditorExtension asset in prefab.RootGameObject_C1001.GetAsset(file).FetchHierarchy())
				{
					yield return asset;
				}
			}
			else if (prefab.Has_Objects_C1001())//DataTemplate
			{
				foreach (PPtr_EditorExtension__3_0_0_f5 asset in prefab.Objects_C1001)
				{
					yield return asset.GetAsset(file);
				}
			}
		}
	}
}
