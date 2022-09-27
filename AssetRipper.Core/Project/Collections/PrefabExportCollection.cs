using AssetRipper.Core.Classes.Meta;
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
using AssetRipper.SourceGenerated.Classes.ClassID_468431735;
using System.Collections.Generic;

namespace AssetRipper.Core.Project.Collections
{
	public sealed class PrefabExportCollection : AssetsExportCollection
	{
		public PrefabExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, IUnityObjectBase asset) 
			: this(assetExporter, virtualFile, GetRootGameObject(asset)) { }

		private PrefabExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, IGameObject root) 
			: this(assetExporter, root, CreateVirtualPrefab(virtualFile, root)) { }

		private PrefabExportCollection(IAssetExporter assetExporter, IGameObject root, IPrefabInstance? prefab) 
			: base(assetExporter, (IUnityObjectBase?)prefab ?? root)
		{
			RootGameObject = root;
			Prefab = prefab;
			m_file = root.SerializedFile;

			foreach (IEditorExtension asset in root.FetchHierarchy())
			{
				AddAsset(asset);

				if (prefab is not null)
				{
					SetPrefabInternal(asset, prefab);
				}
			}
		}

		private static void SetPrefabInternal(IEditorExtension asset, IPrefabInstance prefab)
		{
			if (asset is IGameObject gameObject)
			{
				if (gameObject.Has_PrefabAsset_C1())
				{
					gameObject.PrefabAsset_C1.CopyValues(prefab.SerializedFile.CreatePPtr(prefab));
				}
			}
			else if (asset is IComponent component)
			{
				if (component.Has_PrefabAsset_C2())
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
				return component.GameObject_C2.TryGetAsset(component.SerializedFile) is not null;
			}
			return false;
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => PrefabKeyword;

		private static IGameObject GetRootGameObject(IUnityObjectBase asset)
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
		private ISerializedFile m_file;
		public override TransferInstructionFlags Flags => base.Flags | TransferInstructionFlags.SerializeForPrefabSystem;
		public IGameObject RootGameObject { get; }
		public IPrefabInstance? Prefab { get; }
		public override string Name => RootGameObject.NameString;

		private static IPrefabInstance? CreateVirtualPrefab(VirtualSerializedFile virtualFile, IGameObject root)
		{
			//Prior to 2018.3, Prefab was an actual asset inside "*.prefab" files.
			//After that, PrefabImporter and PrefabInstance were introduced as a replacement.
			if (root.Has_PrefabInstance_C1())
			{
				return null;
			}
			else
			{
				IPrefabInstance instance = virtualFile.CreateAsset<IPrefabInstance>(ClassIDType.PrefabInstance, root.SerializedFile.Version);
				instance.RootGameObject_C1001?.CopyValues(root.SerializedFile.CreatePPtr(root));
				instance.IsPrefabAsset_C1001 = true;
				if (instance is IHasNameString hasName)
				{
					hasName.NameString = root.NameString;
				}
				if (instance.Has_Objects_C1001())
				{
					foreach (IEditorExtension editorExtension in root.FetchHierarchy())
					{
						instance.Objects_C1001.AddNew().CopyValues(root.SerializedFile.CreatePPtr(editorExtension));
					}
				}
				instance.AssetBundleName = root.AssetBundleName;
				instance.OriginalDirectory = root.OriginalDirectory;
				instance.OriginalName = root.OriginalName;
				instance.OriginalExtension = root.OriginalExtension;
				return instance;
			}
		}

		public override IEnumerable<IUnityObjectBase> Assets
		{
			get
			{
				if (Prefab is not null)
				{
					//Required to make Prefab.RootGameObject_C1001 or DataTemplate.Objects_C1001 export correctly
					//because VirtualSerializedFile can't reference other files.
					m_file = RootGameObject.SerializedFile;
					yield return Prefab;
				}

				foreach (IUnityObjectBase asset in m_assets)
				{
					m_file = asset.SerializedFile;
					yield return asset;
				}
			}
		}

		protected override IUnityObjectBase CreateImporter(IExportContainer container)
		{
			if (Prefab is null)
			{
				IPrefabImporter importer = PrefabImporterFactory.CreateAsset(container.ExportVersion);
				if (RootGameObject.AssetBundleName is not null)
				{
					importer.AssetBundleName_C468431735.String = RootGameObject.AssetBundleName;
				}
				return importer;
			}
			else
			{
				return base.CreateImporter(container);
			}
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			if (Prefab is not null)
			{
				return base.GetExportID(asset);
			}
			return m_exportIDs[asset.AssetInfo];
		}

		/// <summary>
		/// Used for <see cref="IPrefabInstance.SourcePrefab_C1001"/>
		/// </summary>
		/// <returns></returns>
		public MetaPtr GenerateMetaPtrForPrefab()
		{
			return new MetaPtr(
				ExportIdHandler.GetMainExportID((uint)ClassIDType.PrefabInstance), 
				Asset.GUID, 
				Prefab is null ? AssetType.Meta : AssetType.Serialized);
		}

		public const string PrefabKeyword = "prefab";
	}
}
