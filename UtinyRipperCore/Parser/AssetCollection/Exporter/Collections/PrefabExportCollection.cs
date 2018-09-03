using System.Collections.Generic;
using System.Linq;
using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public sealed class PrefabExportCollection : AssetsExportCollection, IComparer<Object>
	{
		public PrefabExportCollection(IAssetExporter assetExporter, Object asset) :
			this(assetExporter, CreatePrefab(asset))
		{
		}

		private PrefabExportCollection(IAssetExporter assetExporter, Prefab prefab) :
			base(assetExporter, prefab)
		{
			foreach (EditorExtension asset in prefab.FetchObjects())
			{
				AddAsset(asset);
			}
		}

		private static Prefab CreatePrefab(Object asset)
		{
			GameObject go;
			if(asset.ClassID == ClassIDType.GameObject)
			{
				go = (GameObject)asset;
			}
			else
			{
				Component component = (Component)asset;
				go = component.GameObject.GetAsset(component.File);
			}
			GameObject root = go.GetRoot();

			Prefab prefab = new Prefab(root);
			foreach (EditorExtension comp in prefab.FetchObjects())
			{
				if (comp.ClassID == ClassIDType.GameObject)
				{
					go = (GameObject)comp;
					int depth = go.GetRootDepth();
					comp.ObjectHideFlags = depth > 1 ? 1u : 0u;
				}
				else
				{
					comp.ObjectHideFlags = 1;
				}
				comp.PrefabInternal = prefab.ThisPrefab;
			}
			return prefab;
		}

		public int Compare(Object obj1, Object obj2)
		{
			if (obj1.ClassID == obj2.ClassID)
			{
				return 0;
			}

			if (IsCoreComponent(obj1))
			{
				if (IsCoreComponent(obj2))
				{
					return obj1.ClassID < obj2.ClassID ? -1 : 1;
				}
				else
				{
					return -1;
				}
			}
			else
			{
				if (IsCoreComponent(obj2))
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
		}
		
		private static bool IsCoreComponent(Object component)
		{
			switch (component.ClassID)
			{
				case ClassIDType.GameObject:
				case ClassIDType.Transform:
				case ClassIDType.RectTransform:
					return true;

				default:
					return false;
			}
		}
	}
}
