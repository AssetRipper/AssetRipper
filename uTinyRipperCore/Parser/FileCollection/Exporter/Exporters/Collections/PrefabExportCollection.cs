using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.AssetExporters
{
	public sealed class PrefabExportCollection : AssetsExportCollection
	{
		public PrefabExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, Object asset) :
			this(assetExporter, virtualFile, GetAssetRoot(asset))
		{
		}

		private PrefabExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, GameObject root) :
			this(assetExporter, root.File, Prefab.CreateVirtualInstance(virtualFile, root))
		{
		}

		private PrefabExportCollection(IAssetExporter assetExporter, ISerializedFile file, Prefab prefab) :
			base(assetExporter, prefab)
		{
			foreach (EditorExtension asset in prefab.FetchObjects(file))
			{
				AddAsset(asset);
			}
		}

		public static bool IsValidAsset(Object asset)
		{
			if (asset.ClassID == ClassIDType.GameObject)
			{
				return true;
			}
			Component component = (Component)asset;
			return component.GameObject.FindAsset(component.File) != null;
		}

		protected override string GetExportExtension(Object asset)
		{
			return Prefab.PrefabKeyword;
		}

		private static GameObject GetAssetRoot(Object asset)
		{
			GameObject go;
			if (asset.ClassID == ClassIDType.GameObject)
			{
				go = (GameObject)asset;
			}
			else
			{
				Component component = (Component)asset;
				go = component.GameObject.GetAsset(component.File);
			}

			return go.GetRoot();
		}
		public override ISerializedFile File => m_file;
		public override TransferInstructionFlags Flags => base.Flags | TransferInstructionFlags.SerializeForPrefabSystem;

		// TODO: HACK: prefab's assets may be stored in different files
		// Need to find a way to set a file for current asset nicely
		public override IEnumerable<Object> Assets
		{
			get
			{
				m_file = m_exportIDs.Keys.First().File;
				yield return Asset;

				foreach (Object asset in m_exportIDs.Keys)
				{
					m_file = asset.File;
					yield return asset;
				}
			}
		}

		private ISerializedFile m_file;
	}
}
