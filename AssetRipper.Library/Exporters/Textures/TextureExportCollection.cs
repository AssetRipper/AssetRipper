using AssetRipper.Core;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_1006;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Sprite_;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasData;
using AssetRipper.SourceGenerated.Subclasses.SpriteMetaData;
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
				foreach (ISprite sprite in texture.SerializedFile.Collection.FetchAssetsOfType<ISprite>())
				{
					if (sprite.RD_C213.Texture.IsAsset(sprite.SerializedFile, texture))
					{
						ISpriteAtlas? atlas = sprite.SpriteAtlas_C213?.TryGetAsset(sprite.SerializedFile);
						AddToDictionary(sprite, atlas);
						if (exportSprites)
						{
							AddAsset(sprite);
						}
					}
				}

				foreach (ISpriteAtlas atlas in texture.SerializedFile.Collection.FetchAssetsOfType<ISpriteAtlas>())
				{
					if (atlas.RenderDataMap_C687078895.Count > 0)
					{
						foreach (ISprite? sprite in atlas.PackedSprites_C687078895P)
						{
							if (sprite is not null 
								&& atlas.RenderDataMap_C687078895.TryGetValue(sprite.RenderDataKey_C213!, out ISpriteAtlasData? atlasData)
								&& atlasData.Texture.IsAsset(atlas.SerializedFile, texture))
							{
								AddToDictionary(sprite, atlas);
								if (exportSprites)
								{
									AddAsset(sprite);
								}
							}
						}
					}
				}
			}
		}

		private void AddToDictionary(ISprite sprite, ISpriteAtlas? atlas)
		{
			if (m_sprites.TryGetValue(sprite, out ISpriteAtlas? mappedAtlas))
			{
				if (mappedAtlas is null)
				{
					m_sprites[sprite] = atlas;
				}
				else if (atlas is not null && atlas != mappedAtlas)
				{
					throw new Exception($"{nameof(atlas)} is not the same as {nameof(mappedAtlas)}");
				}
			}
			else
			{
				m_sprites.Add(sprite, atlas);
			}
		}

		public static IExportCollection CreateExportCollection(IAssetExporter assetExporter, ISprite asset)
		{
			if (asset.RD_C213.Texture.TryGetAsset(asset.SerializedFile, out ITexture2D? texture))
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
				AddSprites(container, importer);
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

		private void AddSprites(IExportContainer container, ITextureImporter importer)
		{
			if (m_sprites.Count == 0)
			{
				importer.SpriteMode_C1006 = (int)Core.Classes.Meta.Importers.Texture.SpriteImportMode.Single;
				importer.SpriteExtrude_C1006 = 1;
				importer.SpriteMeshType_C1006 = (int)Core.Classes.Sprite.SpriteMeshType.FullRect;//See pull request #306
				importer.Alignment_C1006 = (int)Core.Classes.Meta.Importers.Texture.SpriteAlignment.Center;
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
					importer.SpriteMode_C1006 = (int)Core.Classes.Meta.Importers.Texture.SpriteImportMode.Single;
				}
				else
				{
					importer.SpriteMode_C1006 = (int)Core.Classes.Meta.Importers.Texture.SpriteImportMode.Multiple;
					importer.TextureType_C1006 = (int)Core.Classes.Meta.Importers.Texture.TextureImporterType.Sprite;
				}
				importer.SpriteExtrude_C1006 = sprite.Extrude_C213;
				importer.SpriteMeshType_C1006 = (int)sprite.RD_C213.GetMeshType();
				importer.Alignment_C1006 = (int)Core.Classes.Meta.Importers.Texture.SpriteAlignment.Custom;
				if (importer.Has_SpritePivot_C1006() && sprite.Has_Pivot_C213())
				{
					importer.SpritePivot_C1006.CopyValues(sprite.Pivot_C213);
				}
				if (importer.Has_SpriteBorder_C1006() && sprite.Has_Border_C213())
				{
					importer.SpriteBorder_C1006.CopyValues(sprite.Border_C213);
				}
				importer.SpritePixelsToUnits_C1006 = sprite.PixelsToUnits_C213;
				importer.TextureType_C1006 = (int)Core.Classes.Meta.Importers.Texture.TextureImporterType.Sprite;
				if (m_exportSprites)
				{
					AddSpriteSheet(container, importer);
					AddIDToName(importer);
				}
			}
			else
			{
				ISprite sprite = m_sprites.Keys.First();
				importer.TextureType_C1006 = (int)Core.Classes.Meta.Importers.Texture.TextureImporterType.Sprite;
				importer.SpriteMode_C1006 = (int)Core.Classes.Meta.Importers.Texture.SpriteImportMode.Multiple;
				importer.SpriteExtrude_C1006 = sprite.Extrude_C213;
				importer.SpriteMeshType_C1006 = (int)sprite.RD_C213.GetMeshType();
				importer.Alignment_C1006 = (int)Core.Classes.Meta.Importers.Texture.SpriteAlignment.Center;
				if (importer.Has_SpritePivot_C1006())
				{
					importer.SpritePivot_C1006.SetValues(0.5f, 0.5f);
				}
				importer.SpritePixelsToUnits_C1006 = sprite.PixelsToUnits_C213;
				importer.TextureType_C1006 = (int)Core.Classes.Meta.Importers.Texture.TextureImporterType.Sprite;
				if (m_exportSprites)
				{
					AddSpriteSheet(container, importer);
					AddIDToName(importer);
				}
			}
		}

		private void AddSpriteSheet(IExportContainer container, ITextureImporter importer)
		{
			if (!importer.Has_SpriteSheet_C1006())
			{
			}
			else if (importer.SpriteMode_C1006 == (int)Core.Classes.Meta.Importers.Texture.SpriteImportMode.Single)
			{
				KeyValuePair<ISprite, ISpriteAtlas?> kvp = m_sprites.First();
				ISpriteMetaData smeta = kvp.Key.GenerateSpriteMetaData(container, kvp.Value);
				importer.SpriteSheet_C1006.CopyFromSpriteMetaData(smeta);
			}
			else
			{
				List<ISpriteMetaData> metadata = new List<ISpriteMetaData>(m_sprites.Count);
				foreach (KeyValuePair<ISprite, ISpriteAtlas?> kvp in m_sprites)
				{
					ISpriteMetaData smeta = kvp.Key.GenerateSpriteMetaData(container, kvp.Value);
					if (smeta.Has_InternalID())
					{
						smeta.InternalID = ObjectUtils.GenerateInternalID();
					}
					metadata.Add(smeta);
				}
				importer.SpriteSheet_C1006.Sprites.AddRange(metadata);
			}
		}

		private void AddIDToName(ITextureImporter importer)
		{
			if (importer.SpriteMode_C1006 == (int)Core.Classes.Meta.Importers.Texture.SpriteImportMode.Multiple)
			{
				if (importer.Has_InternalIDToNameTable_C1006())
				{
					foreach (ISprite sprite in m_sprites.Keys)
					{
#warning TODO: TEMP:
						long exportID = GetExportID(sprite);
						ISpriteMetaData smeta = importer.SpriteSheet_C1006.GetSpriteMetaData(sprite.NameString);
						smeta.InternalID = exportID;
						NullableKeyValuePair<int, long> key = new NullableKeyValuePair<int, long>((int)ClassIDType.Sprite, exportID);
						importer.InternalIDToNameTable_C1006.Add(new(key, sprite.Name));
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

		private readonly Dictionary<ISprite, ISpriteAtlas?> m_sprites = new Dictionary<ISprite, ISpriteAtlas?>();
		private readonly bool m_exportSprites;

		private readonly bool m_convert;
		private uint m_nextExportID = 0;
	}
}
