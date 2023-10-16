using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.IO.Files.Utils;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1034;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects
{
	public class AssetExportCollection<T> : ExportCollection where T : IUnityObjectBase
	{
		public AssetExportCollection(IAssetExporter assetExporter, T mainAsset)
		{
			AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));
			Asset = mainAsset ?? throw new ArgumentNullException(nameof(mainAsset));
			var mainBehaviour = mainAsset as IMonoBehaviour;
			if (mainBehaviour != null && mainBehaviour.IsSceneObject()) throw new Exception($"{mainAsset} should be part of a scene, not its own asset collection");

			List<IUnityObjectBase> components = new();

			components.Add(mainAsset);
			m_exportIDs.Add(mainAsset.AssetInfo, GetExportIDHelper(mainAsset));

			if (isScriptableObject)
			{
				if (mainBehaviour != null)
				{
					RecursiveAddAssets(mainBehaviour, components);
				}
			}

			componentArray = components.ToArray();
		}

		private void RecursiveAddAssets(IMonoBehaviour mainBehaviour, List<IUnityObjectBase> components)
		{
			SerializableStructure? structure = mainBehaviour.Structure as SerializableStructure;

			if (structure == null)
			{
				UnloadedStructure? unloadedStructure = mainBehaviour.Structure as UnloadedStructure;
				structure = unloadedStructure?.LoadStructure();
			}

			if (structure != null)
			{
				var pathIDS = structure.pathIDS;
				if (pathIDS != null)
				{
					foreach (IUnityObjectBase asset in mainBehaviour.Collection)
					{
						if (pathIDS.Contains(asset.PathID) && !components.Contains(asset))
						{
							if (asset.ClassID != (int)ClassIDType.MonoBehaviour)
								continue;
							var subMonoBehavior = asset as IMonoBehaviour;
							if (subMonoBehavior == null || subMonoBehavior.IsScriptableObject() || subMonoBehavior.IsSceneObject())
								continue;

							Console.WriteLine($"adding {asset.PathID} to {Asset.PathID}");
							components.Add(asset);
							m_exportIDs.Add(asset.AssetInfo, GetExportIDHelper(asset));

							RecursiveAddAssets(subMonoBehavior, components);
						}
					}
				}
			}
		}

		private long GetExportIDHelper(IUnityObjectBase asset)
		{
			if (asset.ClassID == (int)ClassIDType.MonoBehaviour)
				return asset.PathID;
			return ExportIdHandler.GetMainExportID(asset);
		}

		public override bool Export(IExportContainer container, string projectDirectory)
		{
			string subPath = Asset.OriginalName is not null || Asset.OriginalDirectory is not null
				? Path.Combine(projectDirectory, DirectoryUtils.FixInvalidPathCharacters(Asset.OriginalDirectory ?? ""))
				: Path.Combine(projectDirectory, AssetsKeyword, Asset.ClassName);
			string fileName = GetUniqueFileName(Asset, subPath);

			Directory.CreateDirectory(subPath);

			string filePath = Path.Combine(subPath, fileName);
			bool result = ExportInner(container, filePath, projectDirectory);
			if (result)
			{
				Meta meta = new Meta(Asset.GUID, CreateImporter(container));
				ExportMeta(container, meta, filePath);
				return true;
			}
			return false;
		}

		public override bool IsContains(IUnityObjectBase asset)
		{
			return m_exportIDs.ContainsKey(asset.AssetInfo);
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			return m_exportIDs[asset.AssetInfo];
		}

		public override MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			long exportID = GetExportID(asset);
			return isLocal ?
				new MetaPtr(exportID) :
				new MetaPtr(exportID, Asset.GUID, AssetExporter.ToExportType(Asset));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="container"></param>
		/// <param name="filePath">The full path to the exported asset destination</param>
		/// <param name="dirPath">The full path to the project export directory</param>
		/// <returns>True if export was successful, false otherwise</returns>
		protected virtual bool ExportInner(IExportContainer container, string filePath, string dirPath)
		{
			if(isScriptableObject)
				return AssetExporter.Export(container, Assets, filePath);
			return AssetExporter.Export(container, Asset, filePath);
		}

		protected virtual IUnityObjectBase CreateImporter(IExportContainer container)
		{
			INativeFormatImporter importer = NativeFormatImporter.Create(container.File, container.ExportVersion);
			importer.MainObjectFileID = GetExportID(Asset);
			if (importer.Has_AssetBundleName_R() && Asset.AssetBundleName is not null)
			{
				importer.AssetBundleName_R = Asset.AssetBundleName;
			}
			return importer;
		}

		public bool isScriptableObject { get => (AssetExporter as ScriptableObjectExporter) != null; }

		public override IAssetExporter AssetExporter { get; }
		public override AssetCollection File => Asset.Collection;
		public override IEnumerable<IUnityObjectBase> Assets
		{
			get {
				foreach (IUnityObjectBase asset in componentArray)
				{
					yield return asset;
				}
			}
		}
		public override string Name => Asset.GetBestName();
		public T Asset { get; }

		private readonly IUnityObjectBase[] componentArray;
		private readonly Dictionary<AssetRipper.Assets.Metadata.AssetInfo, long> m_exportIDs = new();
	}
}
