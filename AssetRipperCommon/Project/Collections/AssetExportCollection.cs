using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Meta.Importers;
using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.VersionHandling;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Project.Collections
{
	public class AssetExportCollection : ExportCollection
	{
		public AssetExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset)
		{
			if (assetExporter == null)
			{
				throw new ArgumentNullException(nameof(assetExporter));
			}
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}
			AssetExporter = assetExporter;
			Asset = asset;
		}

		public AssetExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset, string fileExtension) : this(assetExporter, asset)
		{
			this.fileExtension = fileExtension;
		}

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			string subPath;
			string fileName;
			if (container.TryGetAssetPathFromAssets(Assets, out IUnityObjectBase asset, out string assetPath))
			{
				string resourcePath = Path.Combine(dirPath, $"{assetPath}.{GetExportExtension(asset)}");
				subPath = Path.GetDirectoryName(resourcePath);
				string resFileName = Path.GetFileName(resourcePath);
#warning TODO: combine assets with the same res path into one big asset
				// Unity distinguish assets with non unique path by its type, but file system doesn't support it
				fileName = GetUniqueFileName(subPath, resFileName);
			}
			else
			{
				string subFolder = Asset.ExportPath;
				subPath = Path.Combine(dirPath, subFolder);
				fileName = GetUniqueFileName(container.File, Asset, subPath);
			}

			Directory.CreateDirectory(subPath);

			string filePath = Path.Combine(subPath, fileName);
			bool result = ExportInner(container, filePath, dirPath);
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
			throw new ArgumentException(nameof(asset));
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
		protected virtual bool ExportInner(ProjectAssetContainer container, string filePath, string dirPath)
		{
			return AssetExporter.Export(container, Asset.ConvertLegacy(container), filePath);
		}

		protected virtual IAssetImporter CreateImporter(IExportContainer container)
		{
			INativeFormatImporter importer = VersionManager.GetHandler(container.ExportVersion).ImporterFactory.CreateNativeFormatImporter(container.ExportLayout);
			importer.MainObjectFileID = GetExportID(Asset);
			return importer;
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			if (string.IsNullOrWhiteSpace(fileExtension))
				return base.GetExportExtension(asset);
			else
				return fileExtension;
		}

		private string fileExtension;
		public override IAssetExporter AssetExporter { get; }
		public override ISerializedFile File => Asset.SerializedFile;
		public override IEnumerable<IUnityObjectBase> Assets
		{
			get { yield return Asset; }
		}
		public override string Name
		{
			get
			{
				if (Asset is IHasName hasName)
					return hasName.GetNameNotEmpty();
				else
					return Asset.ToString();
			}
		}
		public IUnityObjectBase Asset { get; }
	}
}
