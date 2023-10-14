using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Processing;
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
		public PrefabExportCollection(IAssetExporter assetExporter, TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
			: this(assetExporter, virtualFile, GetRootGameObjectAndPrefab(asset)) { }

		private PrefabExportCollection(IAssetExporter assetExporter, TemporaryAssetCollection virtualFile, (IGameObject, IPrefabInstance?) rootPrefabPair)
			: base(assetExporter, rootPrefabPair.Item2 ?? CreateVirtualPrefab(virtualFile, rootPrefabPair.Item1))
		{
			RootGameObject = rootPrefabPair.Item1;

			//Prior to 2018.3, Prefab was an actual asset inside "*.prefab" files.
			//After that, PrefabImporter and PrefabInstance were introduced as a replacement.
			Prefab = rootPrefabPair.Item2;
			Debug.Assert(Prefab is null or IPrefabMarker);

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

		private static (IGameObject, IPrefabInstance?) GetRootGameObjectAndPrefab(IUnityObjectBase asset)
		{
			switch (asset)
			{
				case IGameObject gameObject:
					{
						IGameObject root = gameObject.GetRoot();
						return (root, (IPrefabInstance?)root.MainAsset);
					}

				case IComponent component:
					{
						IGameObject root = component.GetGameObject().GetRoot();
						return (root, (IPrefabInstance?)root.MainAsset);
					}

				case IPrefabInstance prefab:
					return (prefab.GetRootGameObject(), prefab);
				default:
					throw new NotSupportedException();
			}
		}

		public override TransferInstructionFlags Flags => base.Flags | TransferInstructionFlags.SerializeForPrefabSystem;
		public IGameObject RootGameObject { get; }
		/// <summary>
		/// Null on 2018.3 and higher
		/// </summary>
		public IPrefabInstance? Prefab { get; }
		public override string Name => RootGameObject.Name;

		/// <summary>
		/// Prior to 2018.3, Prefab was an actual asset inside "*.prefab" files.
		/// After that, PrefabImporter and PrefabInstance were introduced as a replacement.
		/// This code is only for creating a fake <see cref="IPrefabInstanceMarker"/>.
		/// <see cref="IPrefabMarker"/> assets are created inside <see cref="PrefabProcessor"/>.
		/// </summary>
		/// <param name="virtualFile"></param>
		/// <param name="root"></param>
		/// <returns></returns>
		private static IPrefabInstance CreateVirtualPrefab(TemporaryAssetCollection virtualFile, IGameObject root)
		{
			UnityVersion version = UnityVersion.Max(virtualFile.Version, new UnityVersion(2018, 3, 0));
			IPrefabInstance prefab = virtualFile.CreateAsset((int)ClassIDType.PrefabInstance, version, PrefabInstance.Create);
			Debug.Assert(prefab is IPrefabInstanceMarker);
			prefab.RootGameObjectP = root;
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
				IPrefabImporter importer = PrefabImporter.Create(container.File, container.ExportVersion);
				if (RootGameObject.AssetBundleName is not null)
				{
					importer.AssetBundleName_R = RootGameObject.AssetBundleName;
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
		/// Used for <see cref="IPrefabInstance.SourcePrefab"/>
		/// </summary>
		/// <returns></returns>
		public MetaPtr GenerateMetaPtrForPrefab()
		{
			return new MetaPtr(
				ExportIdHandler.GetMainExportID((int)ClassIDType.PrefabInstance),
				Asset.GUID,
				Prefab is null ? AssetType.Meta : AssetType.Serialized);
		}

		public const string PrefabKeyword = "prefab";
	}
}
