using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.Project;
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
				importer.SpriteModeE = SpriteImportMode.Single;
				importer.SpriteExtrude = 1;
				importer.SpriteMeshType = (int)SpriteMeshType.FullRect;//See pull request #306
				importer.Alignment = (int)SpriteAlignment.Center;
				if (importer.Has_SpritePivot())
				{
					importer.SpritePivot.SetValues(0.5f, 0.5f);
				}
				importer.SpritePixelsToUnits = 100.0f;
			}
			else if (textureSpriteInformation.Count == 1)
			{
				ISprite sprite = textureSpriteInformation.Keys.First();
				ITexture2D texture = Asset;
				if (sprite.Rect == sprite.RD.TextureRect && sprite.Name == texture.Name)
				{
					importer.SpriteModeE = SpriteImportMode.Single;
				}
				else
				{
					importer.SpriteModeE = SpriteImportMode.Multiple;
					importer.TextureTypeE = TextureImporterType.Sprite;
				}
				importer.SpriteExtrude = sprite.Extrude;
				importer.SpriteMeshType = (int)sprite.RD.GetMeshType();
				importer.Alignment = (int)SpriteAlignment.Custom;
				if (importer.Has_SpritePivot() && sprite.Has_Pivot())
				{
					importer.SpritePivot.CopyValues(sprite.Pivot);
				}
				if (importer.Has_SpriteBorder() && sprite.Has_Border())
				{
					importer.SpriteBorder.CopyValues(sprite.Border);
				}
				importer.SpritePixelsToUnits = sprite.PixelsToUnits;
				importer.TextureTypeE = TextureImporterType.Sprite;
				if (m_exportSprites)
				{
					AddSpriteSheet(importer, textureSpriteInformation);
					AddIDToName(importer, textureSpriteInformation);
				}
			}
			else
			{
				ISprite sprite = textureSpriteInformation.Keys.First();
				importer.TextureTypeE = TextureImporterType.Sprite;
				importer.SpriteModeE = SpriteImportMode.Multiple;
				importer.SpriteExtrude = sprite.Extrude;
				importer.SpriteMeshType = (int)sprite.RD.GetMeshType();
				importer.Alignment = (int)SpriteAlignment.Center;
				if (importer.Has_SpritePivot())
				{
					importer.SpritePivot.SetValues(0.5f, 0.5f);
				}
				importer.SpritePixelsToUnits = sprite.PixelsToUnits;
				importer.TextureTypeE = TextureImporterType.Sprite;
				if (m_exportSprites)
				{
					AddSpriteSheet(importer, textureSpriteInformation);
					AddIDToName(importer, textureSpriteInformation);
				}
			}
		}

		private static void AddSpriteSheet(ITextureImporter importer, IReadOnlyDictionary<ISprite, ISpriteAtlas?> textureSpriteInformation)
		{
			if (!importer.Has_SpriteSheet())
			{
			}
			else if (importer.SpriteModeE == SpriteImportMode.Single)
			{
				KeyValuePair<ISprite, ISpriteAtlas?> kvp = textureSpriteInformation.First();
				ISpriteMetaData smeta = SpriteMetaData.Create(kvp.Key.Collection.Version);
				smeta.FillSpriteMetaData(kvp.Key, kvp.Value);
				importer.SpriteSheet.CopyFromSpriteMetaData(smeta);
			}
			else
			{
				AccessListBase<ISpriteMetaData> spriteSheetSprites = importer.SpriteSheet.Sprites;
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
			if (importer.SpriteModeE == SpriteImportMode.Multiple)
			{
				if (importer.Has_InternalIDToNameTable())
				{
					foreach (ISprite sprite in textureSpriteInformation.Keys)
					{
#warning TODO: TEMP:
						long exportID = GetExportID(sprite);
						ISpriteMetaData smeta = importer.SpriteSheet.GetSpriteMetaData(sprite.Name);
						smeta.InternalID = exportID;
						AssetPair<AssetPair<int, long>, Utf8String> pair = importer.InternalIDToNameTable.AddNew();
						pair.Key.Key = (int)ClassIDType.Sprite;
						pair.Key.Value = exportID;
						pair.Value = sprite.Name;
					}
				}
				else if (importer.Has_FileIDToRecycleName_AssetDictionary_Int64_Utf8String())
				{
					foreach (ISprite sprite in textureSpriteInformation.Keys)
					{
						long exportID = GetExportID(sprite);
						importer.FileIDToRecycleName_AssetDictionary_Int64_Utf8String.Add(exportID, sprite.Name);
					}
				}
				else if (importer.Has_FileIDToRecycleName_AssetDictionary_Int32_Utf8String())
				{
					foreach (ISprite sprite in textureSpriteInformation.Keys)
					{
						long exportID = GetExportID(sprite);
						importer.FileIDToRecycleName_AssetDictionary_Int32_Utf8String.Add((int)exportID, sprite.Name);
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
