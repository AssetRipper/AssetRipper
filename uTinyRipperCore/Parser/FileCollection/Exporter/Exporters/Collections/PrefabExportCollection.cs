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
			File = file;
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
		public override ISerializedFile File { get; }
		public override TransferInstructionFlags Flags => base.Flags | TransferInstructionFlags.SerializeForPrefabSystem;
	}
}
