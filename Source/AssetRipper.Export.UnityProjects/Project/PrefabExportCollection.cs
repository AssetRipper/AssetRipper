using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_468431735;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.MarkerInterfaces;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects.Project
{
	public sealed class PrefabExportCollection : AssetsExportCollection<IPrefabInstance>
	{
		public PrefabExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset)
			: this(assetExporter, GetRootGameObjectAndPrefab(asset))
		{
		}

		private PrefabExportCollection(IAssetExporter assetExporter, (IGameObject, IPrefabInstance) rootPrefabPair)
			: base(assetExporter, rootPrefabPair.Item2)
		{
			(RootGameObject, Prefab) = rootPrefabPair;

			foreach (IEditorExtension asset in RootGameObject.FetchHierarchy())
			{
				AddAsset(asset);
			}
		}

		/// <summary>
		/// <list>
		/// <listheader>Valid Assets:</listheader>
		/// <item>GameObject</item>
		/// <item>Component attached to a GameObject</item>
		/// <item>Prefab</item>
		/// </list>
		/// </summary>
		/// <param name="asset">The asset being assessed.</param>
		/// <returns>True if the asset can be used.</returns>
		public static bool IsValidAsset(IUnityObjectBase asset)
		{
			return asset switch
			{
				IComponent component => component.GameObject_C2P is not null,
				IGameObject => true,
				IPrefabInstance => true,
				_ => false,
			};
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => PrefabKeyword;

		private static (IGameObject, IPrefabInstance) GetRootGameObjectAndPrefab(IUnityObjectBase asset)
		{
			switch (asset)
			{
				case IGameObject gameObject:
					{
						IGameObject root = gameObject.GetRoot();
						Debug.Assert(root.MainAsset is not null);
						return (root, (IPrefabInstance)root.MainAsset);
					}

				case IComponent component:
					{
						IGameObject root = component.GameObject_C2P!.GetRoot();
						Debug.Assert(root.MainAsset is not null);
						return (root, (IPrefabInstance)root.MainAsset);
					}

				case IPrefabInstance prefab:
					return (prefab.GetRootGameObject(), prefab);
				default:
					throw new NotSupportedException();
			}
		}

		public override TransferInstructionFlags Flags => base.Flags | TransferInstructionFlags.SerializeForPrefabSystem;
		public IGameObject RootGameObject { get; }
		public IPrefabInstance Prefab { get; }
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
					return Assets;
				}
				else
				{
					Debug.Assert(Assets.First() is IPrefabInstanceMarker);
					return Assets.Skip(1);//Skip the PrefabInstance asset
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
				Asset.GUID,
				EmitPrefabAsset ? AssetType.Serialized : AssetType.Meta);
		}

		public const string PrefabKeyword = "prefab";
	}
}
