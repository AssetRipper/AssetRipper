using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects.Project;
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
using System.Linq;

namespace AssetRipper.Export.UnityProjects.Textures;

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
			
			// MainAsset is strictly guaranteed to be a SpriteInformationObject based on constructor injection.
			IReadOnlyDictionary<ISprite, ISpriteAtlas?>? sprites = ((SpriteInformationObject?)Asset.MainAsset)!.Sprites;
			AddSprites(container, importer, sprites);
			
			return importer;
		}
		else
		{
			return ImporterFactory.GenerateIHVImporter(container, texture);
		}
	}

	protected override bool ExportInner(IExportContainer container, string filePath, string dirPath, FileSystem fileSystem)
	{
		return AssetExporter.Export(container, Asset, filePath, fileSystem);
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

	private void AddSprites(IExportContainer container, ITextureImporter importer, IReadOnlyDictionary<ISprite, ISpriteAtlas?>? textureSpriteInformation)
	{
		if (textureSpriteInformation is null || textureSpriteInformation.Count == 0)
		{
			importer.SpriteModeE = SpriteImportMode.Single;
			importer.SpriteExtrude = 1;
			importer.SpriteMeshType = (int)SpriteMeshType.FullRect; // See pull request #306
			importer.Alignment = (int)SpriteAlignment.Center;
			if (importer.Has_SpritePivot())
			{
				importer.SpritePivot.SetValues(0.5f, 0.5f);
			}
			importer.SpritePixelsToUnits = 100.0f;
			return;
		}

		ISprite firstSprite = textureSpriteInformation.Keys.First();
		bool isSingleSprite = textureSpriteInformation.Count == 1;

		// Guarding RD access: RenderData can be stripped or malformed in highly optimized bundles.
		if (isSingleSprite && firstSprite.Has_RD() && firstSprite.Rect == firstSprite.RD.TextureRect && firstSprite.Name == Asset.Name)
		{
			importer.SpriteModeE = SpriteImportMode.Single;
		}
		else
		{
			importer.SpriteModeE = SpriteImportMode.Multiple;
			importer.TextureTypeE = TextureImporterType.Sprite;
		}

		importer.SpriteExtrude = firstSprite.Extrude;
		importer.SpriteMeshType = (int)(firstSprite.Has_RD() ? firstSprite.RD.MeshType : SpriteMeshType.FullRect);
		
		if (isSingleSprite)
		{
			importer.Alignment = (int)SpriteAlignment.Custom;
			if (importer.Has_SpritePivot() && firstSprite.Has_Pivot())
			{
				importer.SpritePivot.CopyValues(firstSprite.Pivot);
			}
			if (importer.Has_SpriteBorder() && firstSprite.Has_Border())
			{
				importer.SpriteBorder.CopyValues(firstSprite.Border);
			}
		}
		else
		{
			importer.Alignment = (int)SpriteAlignment.Center;
			if (importer.Has_SpritePivot())
			{
				importer.SpritePivot.SetValues(0.5f, 0.5f);
			}
		}

		importer.SpritePixelsToUnits = firstSprite.PixelsToUnits;
		
		if (importer.SpriteModeE != SpriteImportMode.Single)
		{
			importer.TextureTypeE = TextureImporterType.Sprite;
		}

		if (m_exportSprites)
		{
			AddSpriteSheet(container, importer, textureSpriteInformation);
			AddIDToName(container, importer, textureSpriteInformation);
		}
	}

	/// <summary>
	/// Generates the SpriteMetaData map for the texture importer.
	/// </summary>
	private void AddSpriteSheet(IExportContainer container, ITextureImporter importer, IReadOnlyDictionary<ISprite, ISpriteAtlas?> textureSpriteInformation)
	{
		if (!importer.Has_SpriteSheet())
		{
			return;
		}

		if (importer.SpriteModeE == SpriteImportMode.Single)
		{
			KeyValuePair<ISprite, ISpriteAtlas?> kvp = textureSpriteInformation.First();
			ISpriteMetaData smeta = SpriteMetaData.Create(kvp.Key.Collection.Version);
			smeta.FillSpriteMetaData(kvp.Key, kvp.Value);
			importer.SpriteSheet.CopyFromSpriteMetaData(smeta);
		}
		else
		{
			AccessListBase<ISpriteMetaData> spriteSheetSprites = importer.SpriteSheet.Sprites;
			spriteSheetSprites.Capacity = textureSpriteInformation.Count; // Pre-allocation eliminates expansion overhead
			
			foreach (KeyValuePair<ISprite, ISpriteAtlas?> kvp in textureSpriteInformation)
			{
				ISpriteMetaData smeta = spriteSheetSprites.AddNew();
				smeta.FillSpriteMetaData(kvp.Key, kvp.Value);
				
				if (smeta.Has_InternalID())
				{
					// Architectural Fix: Resolving and assigning the ExportID directly during instantiation
					// eliminates the O(N^2) string lookup bottleneck previously occurring in AddIDToName.
					smeta.InternalID = GetExportID(container, kvp.Key);
				}
			}
		}
	}

	/// <summary>
	/// Populates Editor Recycle mapping tables (InternalIDToNameTable, etc.).
	/// </summary>
	private void AddIDToName(IExportContainer container, ITextureImporter importer, IReadOnlyDictionary<ISprite, ISpriteAtlas?> textureSpriteInformation)
	{
		if (importer.SpriteModeE != SpriteImportMode.Multiple)
		{
			return;
		}

		if (importer.Has_InternalIDToNameTable())
		{
			importer.InternalIDToNameTable.Capacity = textureSpriteInformation.Count;
			
			foreach (ISprite sprite in textureSpriteInformation.Keys)
			{
				long exportID = GetExportID(container, sprite);
				
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
				long exportID = GetExportID(container, sprite);
				importer.FileIDToRecycleName_AssetDictionary_Int64_Utf8String.Add(exportID, sprite.Name);
			}
		}
		else if (importer.Has_FileIDToRecycleName_AssetDictionary_Int32_Utf8String())
		{
			foreach (ISprite sprite in textureSpriteInformation.Keys)
			{
				long exportID = GetExportID(container, sprite);
				importer.FileIDToRecycleName_AssetDictionary_Int32_Utf8String.Add((int)exportID, sprite.Name);
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
