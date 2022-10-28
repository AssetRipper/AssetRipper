using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_468431735;
using AssetRipper.SourceGenerated.MarkerInterfaces;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AssetRipper.Core.Project.Collections
{
	public sealed class PrefabExportCollection : AssetsExportCollection
	{
		public PrefabExportCollection(IAssetExporter assetExporter, TemporaryAssetCollection virtualFile, IUnityObjectBase asset) 
			: this(assetExporter, virtualFile, GetRootGameObject(asset)) { }

		private PrefabExportCollection(IAssetExporter assetExporter, TemporaryAssetCollection virtualFile, IGameObject root) 
			: this(assetExporter, root, CreateVirtualPrefab(virtualFile, root)) { }

		private PrefabExportCollection(IAssetExporter assetExporter, IGameObject root, IPrefabInstance prefab) 
			: base(assetExporter, prefab)
		{
			RootGameObject = root;

			//Prior to 2018.3, Prefab was an actual asset inside "*.prefab" files.
			//After that, PrefabImporter and PrefabInstance were introduced as a replacement.
			Prefab = NotPrefabInstance(prefab);

			foreach (IEditorExtension asset in root.FetchHierarchy())
			{
				AddAsset(asset);
				if (Prefab is not null)
				{
					//SetPrefabInternal(asset, Prefab);
				}
			}
		}

		private static IPrefabInstance? NotPrefabInstance(IPrefabInstance prefab)
		{
			return prefab is IPrefabMarker or IDataTemplateMarker ? prefab : null;
		}

		private static void SetPrefabInternal(IEditorExtension asset, IPrefabInstance prefab)
		{
			//To do: move prefab creation to a processor
			if (asset is IGameObject gameObject)
			{
				if (gameObject.Has_PrefabInternal_C1())
				{
					gameObject.PrefabInternal_C1P = (IPrefabMarker)prefab;
				}
			}
			else if (asset is IComponent component)
			{
				if (component.Has_PrefabInternal_C2())
				{
					component.PrefabInternal_C2P = (IPrefabMarker)prefab;
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
				return component.GameObject_C2P is not null;
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
		public override TransferInstructionFlags Flags => base.Flags | TransferInstructionFlags.SerializeForPrefabSystem;
		public IGameObject RootGameObject { get; }
		public IPrefabInstance? Prefab { get; }
		public override string Name => RootGameObject.NameString;

		private static IPrefabInstance CreateVirtualPrefab(TemporaryAssetCollection virtualFile, IGameObject root)
		{
			IPrefabInstance prefab = virtualFile.CreateAsset((int)ClassIDType.PrefabInstance,
					(assetInfo) => PrefabInstanceFactory.CreateAsset(root.Collection.Version, assetInfo));
			prefab.RootGameObject_C1001?.CopyValues(virtualFile.ForceCreatePPtr(root));
			prefab.IsPrefabAsset_C1001 = true;
			if (prefab is IHasNameString hasName)
			{
				hasName.NameString = root.NameString;
			}
			if (prefab.Has_Objects_C1001())
			{
				foreach (IEditorExtension editorExtension in root.FetchHierarchy())
				{
					prefab.Objects_C1001.AddNew().CopyValues(virtualFile.ForceCreatePPtr(editorExtension));
				}
			}
			prefab.AssetBundleName = root.AssetBundleName;
			prefab.OriginalDirectory = root.OriginalDirectory;
			prefab.OriginalName = root.OriginalName;
			prefab.OriginalExtension = root.OriginalExtension;
			return prefab;
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

		public override IEnumerable<IUnityObjectBase> Assets
		{
			get
			{
				if (Prefab is null)
				{
					Debug.Assert(base.Assets.First() is IPrefabInstanceMarker);
					return base.Assets.Skip(1);//Skip the PrefabInstance asset
				}
				else
				{
					return base.Assets;
				}
			}
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
