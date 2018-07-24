using System.Collections.Generic;
using System.IO;

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
			if(!asset.IsValid)
			{
				Logger.Instance.Log(LogType.Warning, LogCategory.Export, $"Can't export '{asset}' because it isn't valid");
				return new EmptyExportCollection();
			}

			switch(asset.ClassID)
			{
				case ClassIDType.Texture2D:
				case ClassIDType.Cubemap:
					return new TextureExportCollection(this, asset);

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
