using AssetRipper.Core;
using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.Classes.Meta.Importers.Texture;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Sprite;
using AssetRipper.Core.Classes.SpriteAtlas;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Converters.Texture2D;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Library.Exporters.Textures
{
	public class TextureExportCollection : AssetsExportCollection
	{
		public TextureExportCollection(IAssetExporter assetExporter, ITexture2D texture, bool convert) : base(assetExporter, texture)
		{
			m_convert = convert;
			if (convert)
			{
				foreach (Sprite sprite in texture.SerializedFile.Collection.FetchAssetsOfType<Sprite>())
				{
					if (sprite.RD.Texture.IsAsset(sprite.SerializedFile, texture))
					{
						SpriteAtlas atlas = Sprite.HasRendererData(sprite.SerializedFile.Version) ? sprite.SpriteAtlas.FindAsset(sprite.SerializedFile) : null;
						m_sprites.Add(sprite, atlas);
						AddAsset(sprite);
					}
				}

				foreach (SpriteAtlas atlas in texture.SerializedFile.Collection.FetchAssetsOfType<SpriteAtlas>())
				{
					if (atlas.RenderDataMap.Count > 0)
					{
						foreach (PPtr<Sprite> spritePtr in atlas.PackedSprites)
						{
							Sprite sprite = spritePtr.FindAsset(atlas.SerializedFile);
							if (sprite != null)
							{
								SpriteAtlasData atlasData = atlas.RenderDataMap[sprite.RenderDataKey];
								if (atlasData.Texture.IsAsset(atlas.SerializedFile, texture))
								{
									m_sprites.Add(sprite, atlas);
									AddAsset(sprite);
								}
							}
						}
					}
				}
			}
		}

		public static IExportCollection CreateExportCollection(IAssetExporter assetExporter, Sprite asset)
		{
			ITexture2D texture = asset.RD.Texture.FindAsset(asset.SerializedFile);
			if (texture == null)
			{
				return new FailExportCollection(assetExporter, asset);
			}
			return new TextureExportCollection(assetExporter, texture, true);
		}

		protected override IAssetImporter CreateImporter(IExportContainer container)
		{
			ITexture2D texture = (ITexture2D)Asset;
			if (m_convert)
			{
				TextureImporter importer = Texture2DConverter.GenerateTextureImporter(container, texture);
				AddSprites(container, importer);
				return importer;
			}
			else
			{
				return Texture2DConverter.GenerateIHVImporter(container, texture);
			}
		}

		protected override bool ExportInner(ProjectAssetContainer container, string filePath, string dirPath)
		{
			return AssetExporter.Export(container, Asset, filePath);
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			if (m_convert)
			{
				return FileExtension ?? "png";
			}
			return base.GetExportExtension(asset);
		}

		protected override long GenerateExportID(IUnityObjectBase asset)
		{
			long exportID = ExportIdHandler.GetMainExportID(asset, m_nextExportID);
			m_nextExportID += 2;
			return exportID;
		}

		private void AddSprites(IExportContainer container, TextureImporter importer)
		{
			if (m_sprites.Count == 0)
			{
				importer.SpriteMode = SpriteImportMode.Single;
				importer.SpriteExtrude = 1;
				importer.SpriteMeshType = SpriteMeshType.FullRect;
				importer.Alignment = SpriteAlignment.Center;
				importer.SpritePivot = new Vector2f(0.5f, 0.5f);
				importer.SpriteBorder = new();
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
				importer.SpriteBorder = new();
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
				importer.SpriteSheet = new SpriteSheetMetaData(smeta);
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
						SpriteMetaData smeta = importer.SpriteSheet.GetSpriteMetaData(sprite.Name);
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

		public string FileExtension { get; set; }

		public readonly Dictionary<Sprite, SpriteAtlas> m_sprites = new Dictionary<Sprite, SpriteAtlas>();

		private readonly bool m_convert;
		private uint m_nextExportID = 0;
	}
}
