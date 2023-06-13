using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.IO.Files.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_1034;

namespace AssetRipper.Export.UnityProjects.Project.Collections
{
	public class AssetExportCollection : ExportCollection
	{
		public AssetExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset)
		{
			AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));
			Asset = asset ?? throw new ArgumentNullException(nameof(asset));
		}

		public override bool Export(IExportContainer container, string projectDirectory)
		{
			string subPath;
			string fileName;
			if (Asset.OriginalName is not null || Asset.OriginalDirectory is not null)
			{
				string assetName = Asset.GetBestName();
				string resourcePath = Path.Combine(projectDirectory, DirectoryUtils.FixInvalidPathCharacters(
					Path.Combine(Asset.OriginalDirectory ?? "", $"{assetName}.{GetExportExtension(Asset)}")));
				subPath = Path.GetDirectoryName(resourcePath)!;
				string resFileName = Path.GetFileName(resourcePath);
				fileName = GetUniqueFileName(subPath, resFileName);
			}
			else
			{
				string subFolder = Path.Combine(AssetsKeyword, Asset.ClassName);
				subPath = Path.Combine(projectDirectory, subFolder);
				fileName = GetUniqueFileName(Asset, subPath);
			}

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
			return Asset.AssetInfo == asset.AssetInfo;
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			if (asset.AssetInfo == Asset.AssetInfo)
			{
				return ExportIdHandler.GetMainExportID(Asset);
			}
			throw new ArgumentException(null, nameof(asset));
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
			return AssetExporter.Export(container, Asset, filePath);
		}

		protected virtual IUnityObjectBase CreateImporter(IExportContainer container)
		{
			INativeFormatImporter importer = NativeFormatImporterFactory.CreateAsset(container.ExportVersion, container.File);
			importer.MainObjectFileID_C1034 = GetExportID(Asset);
			if (importer.Has_AssetBundleName_C1034() && Asset.AssetBundleName is not null)
			{
				importer.AssetBundleName_C1034 = Asset.AssetBundleName;
			}
			return importer;
		}

		public override IAssetExporter AssetExporter { get; }
		public override AssetCollection File => Asset.Collection;
		public override IEnumerable<IUnityObjectBase> Assets
		{
			get { yield return Asset; }
		}
		public override string Name => Asset.GetBestName();
		public IUnityObjectBase Asset { get; }
	}
}
