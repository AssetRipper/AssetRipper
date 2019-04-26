using System.Collections.Generic;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.Classes;
using uTinyRipper.Classes.SpriteAtlases;

namespace uTinyRipper.AssetExporters
{
	public class TextureExportCollection : AssetsExportCollection
	{
#warning TODO: optimize (now it is suuuuuuuuper slow)
		public TextureExportCollection(IAssetExporter assetExporter, Texture2D texture, bool convert):
			base(assetExporter, texture, CreateImporter(texture, convert))
		{
			m_convert = convert;
			if (convert)
			{
				TextureImporter textureImporter = (TextureImporter)MetaImporter;
				Dictionary<Sprite, SpriteAtlas> sprites = new Dictionary<Sprite, SpriteAtlas>();
				foreach (Object asset in texture.File.Collection.FetchAssets())
				{
					switch (asset.ClassID)
					{
						case ClassIDType.Sprite:
							{
								Sprite sprite = (Sprite)asset;
								if (sprite.RD.Texture.IsAsset(sprite.File, texture))
								{
									SpriteAtlas atlas = Sprite.IsReadRendererData(sprite.File.Version) ? sprite.SpriteAtlas.FindAsset(sprite.File) : null;
									sprites.Add(sprite, atlas);
									AddAsset(sprite);
								}
							}
							break;

						case ClassIDType.SpriteAtlas:
							{
								SpriteAtlas atlas = (SpriteAtlas)asset;
								foreach (PPtr<Sprite> spritePtr in atlas.PackedSprites)
								{
									Sprite sprite = spritePtr.FindAsset(atlas.File);
									if (sprite != null)
									{
										SpriteAtlasData atlasData = atlas.RenderDataMap[sprite.RenderDataKey];
										if (atlasData.Texture.IsAsset(atlas.File, texture))
										{
											sprites.Add(sprite, atlas);
											AddAsset(sprite);
										}
									}
								}
							}
							break;
					}
				}
				textureImporter.Sprites = sprites;
			}
		}

		public static IExportCollection CreateExportCollection(IAssetExporter assetExporter, Sprite asset)
		{
			Texture2D texture = asset.RD.Texture.FindAsset(asset.File);
			if (texture == null)
			{
				return new FailExportCollection(assetExporter, asset);
			}
			return new TextureExportCollection(assetExporter, texture, true);
		}

		private static IAssetImporter CreateImporter(Texture2D texture, bool convert)
		{
			if (convert)
			{
				return new TextureImporter(texture);
			}
			else
			{
				return new IHVImageFormatImporter(texture);
			}
		}

		protected override bool ExportInner(ProjectAssetContainer container, string filePath)
		{
			return AssetExporter.Export(container, Asset, filePath);
		}

		protected override string GetExportExtension(Object asset)
		{
			if (m_convert)
			{
				return "png";
			}
			return base.GetExportExtension(asset);
		}

		protected override long GenerateExportID(Object asset)
		{
			long exportID = GetMainExportID(asset, m_nextExportID);
			m_nextExportID += 2;
			return exportID;
		}

		private uint m_nextExportID = 0;
		private bool m_convert = false;
	}
}
