using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes;
using uTinyRipper.Converters;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Project
{
	public class AssetExportCollection : ExportCollection
	{
		public AssetExportCollection(IAssetExporter assetExporter, Object asset)
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

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			string subPath;
			string fileName;
			if (container.TryGetAssetPathFromAssets(Assets, out Object asset, out string assetPath))
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

			if (!DirectoryUtils.Exists(subPath))
			{
				DirectoryUtils.CreateVirtualDirectory(subPath);
			}

			string filePath = Path.Combine(subPath, fileName);
			bool result = ExportInner(container, filePath);
			if (result)
			{
				Meta meta = new Meta(Asset.GUID, CreateImporter(container));
				ExportMeta(container, meta, filePath);
				return true;
			}
			return false;
		}

		public override bool IsContains(Object asset)
		{
			return Asset.AssetInfo == asset.AssetInfo;
		}

		public override long GetExportID(Object asset)
		{
			if(asset.AssetInfo == Asset.AssetInfo)
			{
				return GetMainExportID(Asset);
			}
			throw new ArgumentException(nameof(asset));
		}

		public override MetaPtr CreateExportPointer(Object asset, bool isLocal)
		{
			long exportID = GetExportID(asset);
			return isLocal ?
				new MetaPtr(exportID) :
				new MetaPtr(exportID, Asset.GUID, AssetExporter.ToExportType(Asset));
		}

		protected virtual bool ExportInner(ProjectAssetContainer container, string filePath)
		{
			return AssetExporter.Export(container, Asset.Convert(container), filePath);
		}

		protected virtual AssetImporter CreateImporter(IExportContainer container)
		{
			NativeFormatImporter importer = new NativeFormatImporter(container.ExportLayout);
			importer.MainObjectFileID = GetExportID(Asset);
			return importer;
		}

		public override IAssetExporter AssetExporter { get; }
		public override ISerializedFile File => Asset.File;
		public override IEnumerable<Object> Assets
		{
			get { yield return Asset; }
		}
		public override string Name => Asset.ToString();
		public Object Asset { get; }
	}
}
