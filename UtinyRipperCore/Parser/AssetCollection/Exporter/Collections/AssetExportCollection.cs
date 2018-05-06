using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters.Classes;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class AssetExportCollection : ExportCollection
	{
		public AssetExportCollection(IAssetExporter assetExporter, Object asset):
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
			m_metaImporter = metaImporter;
		}

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			string subFolder = Asset.ExportName;
			string subPath = Path.Combine(dirPath, subFolder);
			string fileName = GetUniqueFileName(Asset, subPath);
			string filePath = Path.Combine(subPath, fileName);

			if (!Directory.Exists(subPath))
			{
				Directory.CreateDirectory(subPath);
			}

			ExportInner(container, filePath);
			Meta meta = new Meta(m_metaImporter, Asset.GUID);
			ExportMeta(container, meta, filePath);
			return true;
		}

		public override bool IsContains(Object asset)
		{
			return Asset == asset;
		}

		public override string GetExportID(Object asset)
		{
			if(asset == Asset)
			{
				return GetMainExportID(Asset);
			}
			throw new ArgumentException(nameof(asset));
		}

		public override ExportPointer CreateExportPointer(Object asset, bool isLocal)
		{
			string exportID = GetExportID(asset);
			return isLocal ?
				new ExportPointer(exportID) :
				new ExportPointer(exportID, Asset.GUID, AssetExporter.ToExportType(Asset.ClassID));
		}

		protected virtual void ExportInner(ProjectAssetContainer container, string filePath)
		{
			AssetExporter.Export(container, Asset, filePath);
		}

		public override IAssetExporter AssetExporter { get; }
		public override IEnumerable<Object> Objects
		{
			get { yield return Asset; }
		}
		public override string Name => Asset.ToString();
		public Object Asset { get; }

		private IAssetImporter m_metaImporter;
	}
}
