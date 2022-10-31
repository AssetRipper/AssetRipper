using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Linq;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Utils;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1006;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasData;
using AssetRipper.SourceGenerated.Subclasses.SpriteMetaData;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Library.Exporters.Textures
{
	public class TextureExportCollection : AssetsExportCollection
	{
		public TextureExportCollection(IAssetExporter assetExporter, ITexture2D texture, bool convert, bool exportSprites) : base(assetExporter, texture)
		{
			m_convert = convert;
			// If exportSprites is false, we do not generate sprite sheet into texture importer,
			// yet we still collect sprites into m_sprites to properly set other texture importer settings.
			m_exportSprites = exportSprites;
			if (convert)
			{
				foreach (IUnityObjectBase asset in texture.Collection.Bundle.FetchAssetsInHierarchy())
				{
					if (asset is ISprite sprite)
					{
						if (sprite.RD_C213.Texture.IsAsset(sprite.Collection, texture))
						{
							ISpriteAtlas? atlas = sprite.SpriteAtlas_C213P;
							AddToDictionary(sprite, atlas);
							if (exportSprites)
							{
								AddAsset(sprite);
							}
						}
					}
					else if (asset is ISpriteAtlas atlas && atlas.RenderDataMap_C687078895.Count > 0)
					{
						foreach (ISprite? packedSprite in atlas.PackedSprites_C687078895P)
						{
							if (packedSprite is not null
								&& atlas.RenderDataMap_C687078895.TryGetValue(packedSprite.RenderDataKey_C213!, out ISpriteAtlasData? atlasData)
								&& atlasData.Texture.IsAsset(atlas.Collection, texture))
							{
								AddToDictionary(packedSprite, atlas);
								if (exportSprites)
								{
									AddAsset(packedSprite);
								}
							}
						}
					}
				}
			}
		}

		private void AddToDictionary(ISprite sprite, ISpriteAtlas? atlas)
		{
			AddToDictionary(sprite, atlas, m_sprites);
		}

		private static void AddToDictionary(ISprite sprite, ISpriteAtlas? atlas, Dictionary<ISprite, ISpriteAtlas?> spriteDictionary)
		{
			if (spriteDictionary.TryGetValue(sprite, out ISpriteAtlas? mappedAtlas))
			{
				if (mappedAtlas is null)
				{
					spriteDictionary[sprite] = atlas;
				}
				else if (atlas is not null && atlas != mappedAtlas)
				{
					throw new Exception($"{nameof(atlas)} is not the same as {nameof(mappedAtlas)}");
				}
			}
			else
			{
				spriteDictionary.Add(sprite, atlas);
			}
		}

		public static IExportCollection CreateExportCollection(IAssetExporter assetExporter, ISprite asset)
		{
			if (asset.RD_C213.Texture.TryGetAsset(asset.Collection, out ITexture2D? texture))
			{
				return new TextureExportCollection(assetExporter, texture, true, true);
			}
			return new FailExportCollection(assetExporter, asset);
		}

		protected override IUnityObjectBase CreateImporter(IExportContainer container)
		{
			ITexture2D texture = (ITexture2D)Asset;
			if (m_convert)
			{
				ITextureImporter importer = ImporterFactory.GenerateTextureImporter(container, texture);
				AddSprites(importer);
				return importer;
			}
			else
			{
				return ImporterFactory.GenerateIHVImporter(container, texture);
			}
		}

		protected override bool ExportInner(IProjectAssetContainer container, string filePath, string dirPath)
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

		private void AddSprites(ITextureImporter importer)
		{
			if (m_sprites.Count == 0)
			{
				importer.SpriteMode_C1006E = SpriteImportMode.Single;
				importer.SpriteExtrude_C1006 = 1;
				importer.SpriteMeshType_C1006 = (int)SpriteMeshType.FullRect;//See pull request #306
				importer.Alignment_C1006 = (int)SpriteAlignment.Center;
				if (importer.Has_SpritePivot_C1006())
				{
					importer.SpritePivot_C1006.SetValues(0.5f, 0.5f);
				}
				importer.SpritePixelsToUnits_C1006 = 100.0f;
			}
			else if (m_sprites.Count == 1)
			{
				ISprite sprite = m_sprites.Keys.First();
				ITexture2D texture = (ITexture2D)Asset;
				if (sprite.Rect_C213 == sprite.RD_C213.TextureRect && sprite.NameString == texture.NameString)
				{
					importer.SpriteMode_C1006E = SpriteImportMode.Single;
				}
				else
				{
					importer.SpriteMode_C1006E = SpriteImportMode.Multiple;
					importer.TextureType_C1006E = TextureImporterType.Sprite;
				}
				importer.SpriteExtrude_C1006 = sprite.Extrude_C213;
				importer.SpriteMeshType_C1006 = (int)sprite.RD_C213.GetMeshType();
				importer.Alignment_C1006 = (int)SpriteAlignment.Custom;
				if (importer.Has_SpritePivot_C1006() && sprite.Has_Pivot_C213())
				{
					importer.SpritePivot_C1006.CopyValues(sprite.Pivot_C213);
				}
				if (importer.Has_SpriteBorder_C1006() && sprite.Has_Border_C213())
				{
					importer.SpriteBorder_C1006.CopyValues(sprite.Border_C213);
				}
				importer.SpritePixelsToUnits_C1006 = sprite.PixelsToUnits_C213;
				importer.TextureType_C1006E = TextureImporterType.Sprite;
				if (m_exportSprites)
				{
					AddSpriteSheet(importer);
					AddIDToName(importer);
				}
			}
			else
			{
				ISprite sprite = m_sprites.Keys.First();
				importer.TextureType_C1006E = TextureImporterType.Sprite;
				importer.SpriteMode_C1006E = SpriteImportMode.Multiple;
				importer.SpriteExtrude_C1006 = sprite.Extrude_C213;
				importer.SpriteMeshType_C1006 = (int)sprite.RD_C213.GetMeshType();
				importer.Alignment_C1006 = (int)SpriteAlignment.Center;
				if (importer.Has_SpritePivot_C1006())
				{
					importer.SpritePivot_C1006.SetValues(0.5f, 0.5f);
				}
				importer.SpritePixelsToUnits_C1006 = sprite.PixelsToUnits_C213;
				importer.TextureType_C1006E = TextureImporterType.Sprite;
				if (m_exportSprites)
				{
					AddSpriteSheet(importer);
					AddIDToName(importer);
				}
			}
		}

		private void AddSpriteSheet(ITextureImporter importer)
		{
			if (!importer.Has_SpriteSheet_C1006())
			{
			}
			else if (importer.SpriteMode_C1006E == SpriteImportMode.Single)
			{
				KeyValuePair<ISprite, ISpriteAtlas?> kvp = m_sprites.First();
				ISpriteMetaData smeta = SpriteMetaDataFactory.CreateAsset(kvp.Key.Collection.Version);
				kvp.Key.FillSpriteMetaData(kvp.Value, smeta);
				importer.SpriteSheet_C1006.CopyFromSpriteMetaData(smeta);
			}
			else
			{
				AccessListBase<ISpriteMetaData> spriteSheetSprites = importer.SpriteSheet_C1006.Sprites;
				foreach (KeyValuePair<ISprite, ISpriteAtlas?> kvp in m_sprites)
				{
					ISpriteMetaData smeta = spriteSheetSprites.AddNew();
					kvp.Key.FillSpriteMetaData(kvp.Value, smeta);
					if (smeta.Has_InternalID())
					{
						smeta.InternalID = ObjectUtils.GenerateInternalID();
					}
				}
			}
		}

		private void AddIDToName(ITextureImporter importer)
		{
			if (importer.SpriteMode_C1006E == SpriteImportMode.Multiple)
			{
				if (importer.Has_InternalIDToNameTable_C1006())
				{
					foreach (ISprite sprite in m_sprites.Keys)
					{
#warning TODO: TEMP:
						long exportID = GetExportID(sprite);
						ISpriteMetaData smeta = importer.SpriteSheet_C1006.GetSpriteMetaData(sprite.NameString);
						smeta.InternalID = exportID;
						AssetPair<AssetPair<int, long>, Utf8String> pair = importer.InternalIDToNameTable_C1006.AddNew();
						pair.Key.Key = (int)ClassIDType.Sprite;
						pair.Key.Value = exportID;
						pair.Value.CopyValues(sprite.Name);
					}
				}
				else
				{
					foreach (ISprite sprite in m_sprites.Keys)
					{
						long exportID = GetExportID(sprite);
						if (importer.Has_FileIDToRecycleName_C1006_AssetDictionary_Int32_Utf8String())
						{
							importer.FileIDToRecycleName_C1006_AssetDictionary_Int32_Utf8String.Add((int)exportID, sprite.Name);
						}
						else if (importer.Has_FileIDToRecycleName_C1006_AssetDictionary_Int64_Utf8String())
						{
							importer.FileIDToRecycleName_C1006_AssetDictionary_Int64_Utf8String.Add(exportID, sprite.Name);
						}
					}
				}
			}
		}

		public string? FileExtension { get; set; }

		private readonly Dictionary<ISprite, ISpriteAtlas?> m_sprites = new();
		private readonly bool m_exportSprites;

		private readonly bool m_convert;
		private uint m_nextExportID = 0;
	}
}
