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
			string subPath;
			string fileName;
			if (container.TryGetResourcePathFromAssets(Assets, out Object asset, out string subResourcePath))
			{
				string resourcePath = Path.Combine(dirPath, $"{subResourcePath}.{GetExportExtension(asset)}");
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
			string filePath = Path.Combine(subPath, fileName);

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
