using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.Classes.SpriteAtlases;
using uTinyRipper.Classes.Sprites;
using uTinyRipper.Classes.TextureImporters;
using uTinyRipper.Converters;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Project
{
	public class TextureExportCollection : AssetsExportCollection
	{
#warning TODO: optimize (now it is suuuuuuuuper slow)
		public TextureExportCollection(IAssetExporter assetExporter, Texture2D texture, bool convert):
			base(assetExporter, texture)
		{
			m_convert = convert;
			if (convert)
			{
				foreach (Object asset in texture.File.Collection.FetchAssets())
				{
					switch (asset.ClassID)
					{
						case ClassIDType.Sprite:
							{
								Sprite sprite = (Sprite)asset;
								if (sprite.RD.Texture.IsAsset(sprite.File, texture))
								{
									SpriteAtlas atlas = Sprite.HasRendererData(sprite.File.Version) ? sprite.SpriteAtlas.FindAsset(sprite.File) : null;
									m_sprites.Add(sprite, atlas);
									AddAsset(sprite);
								}
							}
							break;

						case ClassIDType.SpriteAtlas:
							{
								SpriteAtlas atlas = (SpriteAtlas)asset;
								if (atlas.RenderDataMap.Count > 0)
								{
									foreach (PPtr<Sprite> spritePtr in atlas.PackedSprites)
									{
										Sprite sprite = spritePtr.FindAsset(atlas.File);
										if (sprite != null)
										{
											SpriteAtlasData atlasData = atlas.RenderDataMap[sprite.RenderDataKey];
											if (atlasData.Texture.IsAsset(atlas.File, texture))
											{
												m_sprites.Add(sprite, atlas);
												AddAsset(sprite);
											}
										}
									}
								}
							}
							break;
					}
				}
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

		protected override AssetImporter CreateImporter(IExportContainer container)
		{
			Texture2D texture = (Texture2D)Asset;
			if (m_convert)
			{
				TextureImporter importer = texture.GenerateTextureImporter(container);
				AddSprites(container, importer);
				return importer;
			}
			else
			{
				return texture.GenerateIHVImporter(container);
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

		private void AddSprites(IExportContainer container, TextureImporter importer)
		{
			if (m_sprites.Count == 0)
			{
				importer.SpriteMode = SpriteImportMode.Single;
				importer.SpriteExtrude = 1;
				importer.SpriteMeshType = SpriteMeshType.Tight;
				importer.Alignment = SpriteAlignment.Center;
				importer.SpritePivot = new Vector2f(0.5f, 0.5f);
				importer.SpriteBorder = default;
				importer.SpritePixelsToUnits = 100.0f;
			}
			else if (m_sprites.Count == 1)
			{
				Sprite sprite = m_sprites.Keys.First();
				Texture2D texture = (Texture2D)Asset;
				if (sprite.Rect == sprite.RD.TextureRect && sprite.Name == texture.Name)
				{
					importer.SpriteMode = SpriteImportMode.Single;
				}
				else
				{
					importer.TextureType = TextureImporterType.Sprite;
					importer.SpriteMode = SpriteImportMode.Multiple;
				}
				importer.SpriteExtrude = sprite.Extrude;
				importer.SpriteMeshType = sprite.RD.MeshType;
				importer.Alignment = SpriteAlignment.Custom;
				importer.SpritePivot = sprite.Pivot;
				importer.SpriteBorder = sprite.Border;
				importer.SpritePixelsToUnits = sprite.PixelsToUnits;
				importer.TextureType = TextureImporterType.Sprite;
				AddSpriteSheet(container, importer);
				AddIDToName(container, importer);
			}
			else
			{
				Sprite sprite = m_sprites.Keys.First();
				importer.TextureType = TextureImporterType.Sprite;
				importer.SpriteMode = SpriteImportMode.Multiple;
				importer.SpriteExtrude = sprite.Extrude;
				importer.SpriteMeshType = sprite.RD.MeshType;
				importer.Alignment = SpriteAlignment.Center;
				importer.SpritePivot = new Vector2f(0.5f, 0.5f);
				importer.SpriteBorder = default;
				importer.SpritePixelsToUnits = sprite.PixelsToUnits;
				importer.TextureType = TextureImporterType.Sprite;
				AddSpriteSheet(container, importer);
				AddIDToName(container, importer);
			}
		}

		private void AddSpriteSheet(IExportContainer container, TextureImporter importer)
		{
			if (importer.SpriteMode == SpriteImportMode.Single)
			{
				var kvp = m_sprites.First();
				SpriteMetaData smeta = kvp.Key.GenerateSpriteMetaData(container, kvp.Value);
				importer.SpriteSheet = new SpriteSheetMetaData(ref smeta);
			}
			else
			{
				List<SpriteMetaData> metadata = new List<SpriteMetaData>(m_sprites.Count);
				foreach (var kvp in m_sprites)
				{
					SpriteMetaData smeta = kvp.Key.GenerateSpriteMetaData(container, kvp.Value);
					if (SpriteMetaData.HasInternalID(container.ExportVersion))
					{
						smeta.InternalID = ObjectUtils.GenerateInternalID();
					}
					metadata.Add(smeta);
				}
				importer.SpriteSheet.Sprites = metadata.ToArray();
			}
		}

		private void AddIDToName(IExportContainer container, TextureImporter importer)
		{
			if (importer.SpriteMode == SpriteImportMode.Multiple)
			{
				if (AssetImporter.HasInternalIDToNameTable(container.ExportVersion))
				{
					foreach (Sprite sprite in m_sprites.Keys)
					{
#warning TODO: TEMP:
						long exportID = GetExportID(sprite);
						ref SpriteMetaData smeta = ref importer.SpriteSheet.GetSpriteMetaData(sprite.Name);
						smeta.InternalID = exportID;
						Tuple<ClassIDType, long> key = new Tuple<ClassIDType, long>(ClassIDType.Sprite, exportID);
						importer.InternalIDToNameTable.Add(key, sprite.Name);
					}
				}
				else
				{
					foreach (Sprite sprite in m_sprites.Keys)
					{
						long exportID = GetExportID(sprite);
						importer.FileIDToRecycleName.Add(exportID, sprite.Name);
					}
				}
			}
		}

		public readonly Dictionary<Sprite, SpriteAtlas> m_sprites = new Dictionary<Sprite, SpriteAtlas>();

		private readonly bool m_convert;
		private uint m_nextExportID = 0;
	}
}
