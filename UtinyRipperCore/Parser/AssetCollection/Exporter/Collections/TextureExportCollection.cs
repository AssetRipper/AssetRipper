using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public class TextureExportCollection : AssetsExportCollection
	{
		public TextureExportCollection(IAssetExporter assetExporter, Texture2D texture):
			base(assetExporter, texture, CreateImporter(texture))
		{
			if(MetaImporter is TextureImporter textureImporter)
			{
				List<Sprite> sprites = new List<Sprite>();
				foreach (Object asset in texture.File.FetchAssets())
				{
					if (asset.ClassID == ClassIDType.Sprite)
					{
						Sprite sprite = (Sprite)asset;
						if (sprite.RD.Texture.IsObject(texture))
						{
							sprites.Add(sprite);
							AddAsset(sprite);
						}
					}
				}
				textureImporter.Sprites = sprites;
			}
		}

		public static IExportCollection CreateExportCollection(IAssetExporter assetExporter, Sprite asset)
		{
			if (Config.IsConvertTexturesToPNG)
			{
				Texture2D texture = asset.RD.Texture.FindObject(asset.File);
				if(texture != null)
				{
					return new TextureExportCollection(assetExporter, texture);
				}
			}
			return new SkipExportCollection(assetExporter, asset);
		}

		private static IAssetImporter CreateImporter(Texture2D texture)
		{
			if (Config.IsConvertTexturesToPNG)
			{
				return new TextureImporter(texture);
			}
			else
			{
				return new IHVImageFormatImporter(texture);
			}
		}

		public override ExportPointer CreateExportPointer(Object asset, bool isLocal)
		{
			if(Config.IsConvertTexturesToPNG)
			{
				return base.CreateExportPointer(asset, isLocal);
			}
			else
			{
				if(Asset == asset)
				{
					return base.CreateExportPointer(asset, isLocal);
				}
				else
				{
					ulong exportID = GetExportID(asset);
					return new ExportPointer(exportID, EngineGUID.MissingReference, AssetType.Meta);
				}
			}
		}

		protected override string ExportInner(ProjectAssetContainer container, string filePath)
		{
			AssetExporter.Export(container, Asset, filePath);
			return filePath;
		}

		protected override ulong GenerateExportID(Object asset)
		{
			ulong exportID = GetMainExportID(asset, m_nextExportID);
			m_nextExportID += 2;
			return exportID;
		}

		private uint m_nextExportID = 0;
	}
}
