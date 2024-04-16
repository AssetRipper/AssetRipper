using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_468431735;
using AssetRipper.SourceGenerated.MarkerInterfaces;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects.Project
{
	public class PrefabExportCollection : AssetsExportCollection<IPrefabInstance>
	{
		public PrefabExportCollection(IAssetExporter assetExporter, PrefabHierarchyObject prefabHierarchyObject)
			: base(assetExporter, prefabHierarchyObject.Prefab)
		{
			RootGameObject = prefabHierarchyObject.Root;
			Prefab = prefabHierarchyObject.Prefab;
			Hierarchy = prefabHierarchyObject;
			AddAssets(prefabHierarchyObject.Assets);
			AddAsset(prefabHierarchyObject);
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => PrefabKeyword;

		public override TransferInstructionFlags Flags => base.Flags | TransferInstructionFlags.SerializeForPrefabSystem;
		public IGameObject RootGameObject { get; }
		public IPrefabInstance Prefab { get; }
		public PrefabHierarchyObject Hierarchy { get; }
		/// <summary>
		/// Prior to 2018.3, Prefab was an actual asset inside "*.prefab" files.
		/// After that, PrefabImporter and PrefabInstance were introduced as a replacement.
		/// </summary>
		public bool EmitPrefabAsset => Prefab is IPrefabMarker;
		public override string Name => RootGameObject.Name;

		protected override IUnityObjectBase CreateImporter(IExportContainer container)
		{
			if (EmitPrefabAsset)
			{
				return base.CreateImporter(container);
			}
			else
			{
				IPrefabImporter importer = PrefabImporter.Create(container.File, container.ExportVersion);
				if (RootGameObject.AssetBundleName is not null)
				{
					importer.AssetBundleName_R = RootGameObject.AssetBundleName;
				}
				return importer;
			}
		}

		public override IEnumerable<IUnityObjectBase> ExportableAssets
		{
			get
			{
				if (EmitPrefabAsset)
				{
					foreach (IUnityObjectBase asset in Hierarchy.Assets)
					{
						m_file = asset.Collection;
						yield return asset;
					}
				}
				else
				{
					Debug.Assert(Hierarchy.Assets.Last() is IPrefabInstanceMarker);
					foreach (IUnityObjectBase asset in Hierarchy.Assets.SkipLast(1))//Skip the PrefabInstance asset
					{
						m_file = asset.Collection;
						yield return asset;
					}
				}
			}
		}

		/// <summary>
		/// Used for <see cref="IPrefabInstance.SourcePrefab"/>
		/// </summary>
		/// <returns></returns>
		public MetaPtr GenerateMetaPtrForPrefab()
		{
			return new MetaPtr(
				ExportIdHandler.GetMainExportID((int)ClassIDType.PrefabInstance),
				GUID,
				EmitPrefabAsset ? AssetType.Serialized : AssetType.Meta);
		}

		public const string PrefabKeyword = "prefab";
	}
}
