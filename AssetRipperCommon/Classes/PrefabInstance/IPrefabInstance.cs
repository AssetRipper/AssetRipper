using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.PrefabInstance
{
	/// <summary>
	/// PrefabInstance - versions &gt;= 2018.3<br/>
	/// Prefab - versions &gt;= 3.5<br/>
	/// DataTemplate - versions &lt; 3.5
	/// </summary>
	public interface IPrefabInstance : IUnityObjectBase
	{
		/// <summary>
		/// Only on versions &gt;= 3.5
		/// </summary>
		PPtr<IGameObject> RootGameObjectPtr { get; set; }
		/// <summary>
		/// m_SourcePrefab - versions &gt;= 2018.2<br/>
		/// m_ParentPrefab - versions &gt;= 3.5<br/>
		/// m_Father - versions &lt; 3.5
		/// </summary>
		PPtr<IPrefabInstance> SourcePrefabPtr { get; set; }
		/// <summary>
		/// Only on versions &lt; 2018.3<br/>
		/// m_IsPrefabAsset - versions &gt;= 2018.2<br/>
		/// m_IsPrefabParent - versions &gt;= 3.5<br/>
		/// m_IsDataTemplate - versions &lt; 3.5
		/// </summary>
		bool IsPrefabAsset { get; set; }
	}

	public static class PrefabInstanceExtensions
	{
		public static string GetName(this IPrefabInstance prefab, IAssetContainer file)
		{
			if (prefab is IHasName hasName && string.IsNullOrEmpty(hasName.Name))
			{
				return hasName.Name;
			}
			else
			{
				return prefab.RootGameObjectPtr.TryGetAsset(file)?.Name;
			}
		}

		public static IEnumerable<IEditorExtension> FetchObjects(this IPrefabInstance prefab, IAssetContainer file)
		{
#warning TEMP HACK:
			//if (file.Layout.PrefabInstance.IsModificationFormat)
			{
				foreach (IEditorExtension asset in prefab.RootGameObjectPtr.GetAsset(file).FetchHierarchy())
				{
					yield return asset;
				}
			}
			/*else
			{
				foreach (PPtr<EditorExtension> asset in Objects)
				{
					yield return asset.GetAsset(file);
				}
			}*/
		}
	}
}
