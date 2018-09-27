using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.AssetExporters
{
	public sealed class PrefabExportCollection : AssetsExportCollection, IComparer<Object>
	{
		public PrefabExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, Object asset) :
			this(assetExporter, asset.File, CreatePrefab(virtualFile, asset))
		{
		}

		private PrefabExportCollection(IAssetExporter assetExporter, ISerializedFile file, Prefab prefab) :
			base(assetExporter, prefab)
		{
			File = file;
			Flags = file.Flags | TransferInstructionFlags.SerializeForPrefabSystem;
			foreach (EditorExtension asset in prefab.FetchObjects(file))
			{
				AddAsset(asset);
			}
		}

		private static Prefab CreatePrefab(VirtualSerializedFile virtualFile, Object asset)
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
			return Prefab.CreateVirtualInstance(virtualFile, root);
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

		public override ISerializedFile File { get; }
		public override TransferInstructionFlags Flags { get; }
	}
}
