using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Classes.PrefabInstance;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Project.Collections
{
	public sealed class PrefabExportCollection : AssetsExportCollection
	{
		public PrefabExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, IUnityObjectBase asset) : this(assetExporter, virtualFile, GetAssetRoot(asset)) { }

		private PrefabExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, IGameObject root) : this(assetExporter, root.File, CreateVirtualPrefab(virtualFile, root)) { }

		private PrefabExportCollection(IAssetExporter assetExporter, IAssetContainer file, IPrefabInstance prefab) : base(assetExporter, prefab)
		{
			foreach (IEditorExtension asset in prefab.FetchObjects(file))
			{
				AddAsset(asset);
			}
		}

		public static bool IsValidAsset(IUnityObjectBase asset)
		{
			if(asset is IGameObject)
			{
				return true;
			}
			else if(asset is IComponent component)
			{
				return component.GameObjectPtr.FindAsset(component.File) != null;
			}
			return false;
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			return PrefabKeyword;
		}

		private static IGameObject GetAssetRoot(IUnityObjectBase asset)
		{
			if (asset is IGameObject gameObject)
			{
				return gameObject.GetRoot();
			}
			else if(asset is IComponent component)
			{
				IGameObject go = component.GameObjectPtr.GetAsset(component.File);
				return go.GetRoot();
			}
			else
			{
				return default;
			}
		}
		public override ISerializedFile File => m_file;
		public override TransferInstructionFlags Flags => base.Flags | TransferInstructionFlags.SerializeForPrefabSystem;

		private static IPrefabInstance CreateVirtualPrefab(VirtualSerializedFile virtualFile, IGameObject root)
		{
			IPrefabInstance instance = virtualFile.CreateAsset<IPrefabInstance>(ClassIDType.PrefabInstance);
			instance.ObjectHideFlags = HideFlags.HideInHierarchy;
			instance.RootGameObjectPtr = root.File.CreatePPtr(root);
			instance.IsPrefabAsset = true;
			if(instance is IHasName hasName)
			{
				hasName.Name = root.Name;
			}
			return instance;
		}

#warning TODO:
		// HACK: prefab's assets may be stored in different files
		// Need to find a way to set a file for current asset nicely
		public override IEnumerable<IUnityObjectBase> Assets
		{
			get
			{
				m_file = m_exportIDs.Keys.First().File;
				yield return Asset;

				foreach (IUnityObjectBase asset in m_assets)
				{
					m_file = asset.File;
					yield return asset;
				}
			}
		}

		public const string PrefabKeyword = "prefab";

		private ISerializedFile m_file;
	}
}
