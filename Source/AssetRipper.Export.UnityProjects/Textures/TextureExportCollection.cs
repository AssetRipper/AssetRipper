using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Primitives;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1006;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.SpriteMetaData;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects.Textures
{
	public class TextureExportCollection : AssetsExportCollection<ITexture2D>
	{
		public TextureExportCollection(TextureAssetExporter assetExporter, SpriteInformationObject spriteInformationObject, bool exportSprites)
			: base(assetExporter, spriteInformationObject.Texture)
		{
			m_exportSprites = exportSprites;

			if (exportSprites && spriteInformationObject.Sprites.Count > 0)
			{
				foreach ((ISprite? sprite, ISpriteAtlas? _) in spriteInformationObject.Sprites)
				{
					Debug.Assert(sprite.TryGetTexture() == Asset);
					AddAsset(sprite);
				}
			}
			AddAsset(spriteInformationObject);
		}

		protected override IUnityObjectBase CreateImporter(IExportContainer container)
		{
			ITexture2D texture = Asset;
			if (m_convert)
			{
				ITextureImporter importer = ImporterFactory.GenerateTextureImporter(container, texture);
				AddSprites(importer, ((SpriteInformationObject?)Asset.MainAsset)!.Sprites);
				return importer;
			}
			else
			{
				return ImporterFactory.GenerateIHVImporter(container, texture);
			}
		}

		protected override bool ExportInner(IExportContainer container, string filePath, string dirPath)
		{
			return AssetExporter.Export(container, Asset, filePath);
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			if (m_convert)
			{
				return ((TextureAssetExporter)AssetExporter).ImageExportFormat.GetFileExtension();
			}
			return base.GetExportExtension(asset);
		}

		protected override long GenerateExportID(IUnityObjectBase asset)
		{
			long exportID = ExportIdHandler.GetMainExportID(asset, m_nextExportID);
			m_nextExportID += 2;
			return exportID;
		}

		private void AddSprites(ITextureImporter importer, IReadOnlyDictionary<ISprite, ISpriteAtlas?>? textureSpriteInformation)
		{
			if (textureSpriteInformation == null || textureSpriteInformation.Count == 0)
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
			else if (textureSpriteInformation.Count == 1)
			{
				ISprite sprite = textureSpriteInformation.Keys.First();
				ITexture2D texture = Asset;
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
					AddSpriteSheet(importer, textureSpriteInformation);
					AddIDToName(importer, textureSpriteInformation);
				}
			}
			else
			{
				ISprite sprite = textureSpriteInformation.Keys.First();
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
					AddSpriteSheet(importer, textureSpriteInformation);
					AddIDToName(importer, textureSpriteInformation);
				}
			}
		}

		private static void AddSpriteSheet(ITextureImporter importer, IReadOnlyDictionary<ISprite, ISpriteAtlas?> textureSpriteInformation)
		{
			if (!importer.Has_SpriteSheet_C1006())
			{
			}
			else if (importer.SpriteMode_C1006E == SpriteImportMode.Single)
			{
				KeyValuePair<ISprite, ISpriteAtlas?> kvp = textureSpriteInformation.First();
				ISpriteMetaData smeta = SpriteMetaDataFactory.CreateAsset(kvp.Key.Collection.Version);
				smeta.FillSpriteMetaData(kvp.Key, kvp.Value);
				importer.SpriteSheet_C1006.CopyFromSpriteMetaData(smeta);
			}
			else
			{
				AccessListBase<ISpriteMetaData> spriteSheetSprites = importer.SpriteSheet_C1006.Sprites;
				foreach (KeyValuePair<ISprite, ISpriteAtlas?> kvp in textureSpriteInformation)
				{
					ISpriteMetaData smeta = spriteSheetSprites.AddNew();
					smeta.FillSpriteMetaData(kvp.Key, kvp.Value);
					if (smeta.Has_InternalID())
					{
						smeta.InternalID = ExportIdHandler.GetInternalId();
					}
				}
			}
		}

		private void AddIDToName(ITextureImporter importer, IReadOnlyDictionary<ISprite, ISpriteAtlas?> textureSpriteInformation)
		{
			if (importer.SpriteMode_C1006E == SpriteImportMode.Multiple)
			{
				if (importer.Has_InternalIDToNameTable_C1006())
				{
					foreach (ISprite sprite in textureSpriteInformation.Keys)
					{
#warning TODO: TEMP:
						long exportID = GetExportID(sprite);
						ISpriteMetaData smeta = importer.SpriteSheet_C1006.GetSpriteMetaData(sprite.NameString);
						smeta.InternalID = exportID;
						AssetPair<AssetPair<int, long>, Utf8String> pair = importer.InternalIDToNameTable_C1006.AddNew();
						pair.Key.Set((int)ClassIDType.Sprite, exportID);
						pair.Value = sprite.Name;
					}
				}
				else if (importer.Has_FileIDToRecycleName_C1006_AssetDictionary_Int64_Utf8String())
				{
					foreach (ISprite sprite in textureSpriteInformation.Keys)
					{
						long exportID = GetExportID(sprite);
						importer.FileIDToRecycleName_C1006_AssetDictionary_Int64_Utf8String.Add(exportID, sprite.Name);
					}
				}
				else if (importer.Has_FileIDToRecycleName_C1006_AssetDictionary_Int32_Utf8String())
				{
					foreach (ISprite sprite in textureSpriteInformation.Keys)
					{
						long exportID = GetExportID(sprite);
						importer.FileIDToRecycleName_C1006_AssetDictionary_Int32_Utf8String.Add((int)exportID, sprite.Name);
					}
				}
			}
		}

		/// <summary>
		/// If exportSprites is false, we do not generate sprite sheet into texture importer,
		/// yet we still need the sprites to properly set other texture importer settings.
		/// </summary>
		private readonly bool m_exportSprites;
		private readonly bool m_convert = true;
		private uint m_nextExportID = 0;
	}
}
