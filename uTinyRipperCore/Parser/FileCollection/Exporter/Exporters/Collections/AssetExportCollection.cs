using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.AssetExporters
{
	public class AssetExportCollection : ExportCollection
	{
		public AssetExportCollection(IAssetExporter assetExporter, Object asset) :
			this(assetExporter, asset, new NativeFormatImporter(asset))
		{
		}

		public AssetExportCollection(IAssetExporter assetExporter, Object asset, IAssetImporter metaImporter)
		{
			if (assetExporter == null)
			{
				throw new ArgumentNullException(nameof(assetExporter));
			}
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}
			if (metaImporter == null)
			{
				throw new ArgumentNullException(nameof(metaImporter));
			}
			AssetExporter = assetExporter;
			Asset = asset;
			MetaImporter = metaImporter;
		}

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			string subFolder = Asset.ExportName;
			string subPath = Path.Combine(dirPath, subFolder);
			string fileName = GetUniqueFileName(container.File, Asset, subPath);
			string filePath = Path.Combine(subPath, fileName);

			if (container.GetResourcePathFromAssets(Assets, filePath, out string resourcePath))
			{
				subPath = Path.GetDirectoryName(resourcePath);
				filePath = resourcePath;
			}

			if (!DirectoryUtils.Exists(subPath))
			{
				DirectoryUtils.CreateVirtualDirectory(subPath);
			}

			if (ExportInner(container, filePath))
			{
				Meta meta = new Meta(MetaImporter, Asset.GUID);
				ExportMeta(container, meta, filePath);
				return true;
			}
			return false;
		}

		public override bool IsContains(Object asset)
		{
			return Asset == asset;
		}

		public override long GetExportID(Object asset)
		{
			if(asset == Asset)
			{
				return GetMainExportID(Asset);
			}
			throw new ArgumentException(nameof(asset));
		}

		public override ExportPointer CreateExportPointer(Object asset, bool isLocal)
		{
			long exportID = GetExportID(asset);
			return isLocal ?
				new ExportPointer(exportID) :
				new ExportPointer(exportID, Asset.GUID, AssetExporter.ToExportType(Asset));
		}

		protected virtual bool ExportInner(ProjectAssetContainer container, string filePath)
		{
			return AssetExporter.Export(container, Asset, filePath);
		}

		public override IAssetExporter AssetExporter { get; }
		public override ISerializedFile File => Asset.File;
		public override IEnumerable<Object> Assets
		{
			get { yield return Asset; }
		}
		public override string Name => Asset.ToString();
		public Object Asset { get; }

		protected IAssetImporter MetaImporter { get; }
	}
}
