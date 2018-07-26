using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class BinaryAssetExporter : IAssetExporter
	{
		public void Export(IExportContainer container, Object asset, string path)
		{
			using (FileStream fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
			{
				asset.ExportBinary(container, fileStream);
			}
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			foreach (Object asset in assets)
			{
				Export(container, asset, path);
			}
		}

		public IExportCollection CreateCollection(Object asset)
		{
			switch(asset.ClassID)
			{
				case ClassIDType.Sprite:
					return TextureExportCollection.CreateExportCollection(this, (Sprite)asset);

				case ClassIDType.Texture2D:
				case ClassIDType.Cubemap:
					return new TextureExportCollection(this, (Texture2D)asset);

				default:
					return new AssetExportCollection(this, asset);
			}
		}

		public AssetType ToExportType(ClassIDType classID)
		{
			return AssetType.Meta;
		}
	}
}
