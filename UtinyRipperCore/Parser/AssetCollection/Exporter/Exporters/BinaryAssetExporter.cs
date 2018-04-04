using System;
using System.IO;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class BinaryAssetExporter : AssetExporter
	{
		public override IExportCollection CreateCollection(UtinyRipper.Classes.Object @object)
		{
			switch (@object.ClassID)
			{
				case ClassIDType.Texture2D:
				case ClassIDType.Cubemap:
					return new TextureExportCollection(this, @object);

				default:
					return new AssetExportCollection(this, @object);
			}
		}

		public override bool Export(IAssetsExporter exporter, IExportCollection collection, string dirPath)
		{
			AssetExportCollection asset = (AssetExportCollection)collection;
			exporter.File = asset.Asset.File;

			string subFolder = asset.Asset.ClassID.ToString();
			string subPath = Path.Combine(dirPath, subFolder);
			string fileName = GetUniqueFileName(asset.Asset, subPath);
			string filePath = Path.Combine(subPath, fileName);

			if(!Directory.Exists(subPath))
			{
				Directory.CreateDirectory(subPath);
			}

			exporter.File = asset.Asset.File;
			using (FileStream fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
			{
				asset.Asset.ExportBinary(exporter, fileStream);
			}

			ExportMeta(exporter, asset, filePath);
			return true;
		}

		public override AssetType ToExportType(ClassIDType classID)
		{
			return AssetType.Meta;
		}
	}
}
