using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Project.Collections
{
	public sealed class PrefabExportCollection : AssetsExportCollection
	{
		public PrefabExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, IUnityObjectBase asset) : this(assetExporter, virtualFile, GetAssetRoot(asset)) { }

		private PrefabExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, IGameObject root) : this(assetExporter, root.SerializedFile, CreateVirtualPrefab(virtualFile, root)) { }

		private PrefabExportCollection(IAssetExporter assetExporter, IAssetContainer file, IPrefabInstance prefab) : base(assetExporter, prefab)
		{
			foreach (IEditorExtension asset in prefab.FetchObjects(file))
			{
				AddAsset(asset);

				//This section might not be necessary. This seems to be quite different from normal prefab files
				SetPrefabInstance(asset, prefab);
			}
		}

		private static void SetPrefabInstance(IEditorExtension asset, IPrefabInstance prefab)
		{
			if (asset is IGameObject gameObject)
			{
				if (gameObject.Has_PrefabInstance_C1())//2018.3 +
				{
					gameObject.PrefabInstance_C1.CopyValues(prefab.SerializedFile.CreatePPtr(prefab));
				}
				else if (gameObject.Has_PrefabAsset_C1())//3.5 +
				{
					gameObject.PrefabAsset_C1.CopyValues(prefab.SerializedFile.CreatePPtr(prefab));
				}
			}
			else if (asset is IComponent component)
			{

				if (component.Has_PrefabInstance_C2())//2018.3 +
				{
					component.PrefabInstance_C2.CopyValues(prefab.SerializedFile.CreatePPtr(prefab));
				}
				else if (component.Has_PrefabAsset_C2())//3.5 +
				{
					component.PrefabAsset_C2.CopyValues(prefab.SerializedFile.CreatePPtr(prefab));
				}
			}
		}

		public static bool IsValidAsset(IUnityObjectBase asset)
		{
			if (asset is IGameObject)
			{
				return true;
			}
			else if (asset is IComponent component)
			{
				return component.GameObject_C2.FindAsset(component.SerializedFile) is not null;
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
			else if (asset is IComponent component)
			{
				return component.GetGameObject().GetRoot();
			}
			else
			{
				throw new NotSupportedException();
			}
		}
		public override ISerializedFile File => m_file;
		public override TransferInstructionFlags Flags => base.Flags | TransferInstructionFlags.SerializeForPrefabSystem;

		//This might not be necessary. 2019.4.3 doesn't use this in normally created prefabs
		private static IPrefabInstance CreateVirtualPrefab(VirtualSerializedFile virtualFile, IGameObject root)
		{
			IPrefabInstance instance = virtualFile.CreateAsset<IPrefabInstance>(ClassIDType.PrefabInstance);
			instance.RootGameObject_C1001?.CopyValues(root.SerializedFile.CreatePPtr(root));
			instance.IsPrefabAsset_C1001 = true;
			if (instance is IHasNameString hasName)
			{
				hasName.NameString = root.NameString;
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
					m_file = asset.SerializedFile;
					yield return asset;
				}
			}
		}

		public const string PrefabKeyword = "prefab";

		private ISerializedFile m_file;
	}
}
