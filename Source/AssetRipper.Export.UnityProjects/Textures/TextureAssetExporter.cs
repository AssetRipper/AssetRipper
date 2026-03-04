using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.Modules.Textures;
using AssetRipper.Import.Logging;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Export.UnityProjects.Textures;

public class TextureAssetExporter : BinaryAssetExporter
{
	public ImageExportFormat ImageExportFormat { get; private set; }
	private SpriteExportMode SpriteExportMode { get; set; }
	private bool ExportSprites => SpriteExportMode is not SpriteExportMode.Yaml;
	
	// Added to check for Lightmap settings
	private LightmapTextureExportFormat LightmapFormat { get; }

	public TextureAssetExporter(FullConfiguration configuration)
	{
		ImageExportFormat = configuration.ExportSettings.ImageExportFormat;
		SpriteExportMode = configuration.ExportSettings.SpriteExportMode;
		LightmapFormat = configuration.ExportSettings.LightmapTextureExportFormat;
	}

	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		// --- FIX FOR LIGHTMAPS ---
		// If it is a Lightmap and we want YAML, return false to force fallback to YamlStreamedAssetExporter
		if (LightmapFormat == LightmapTextureExportFormat.Yaml && asset.MainAsset is SourceGenerated.Classes.ClassID_1120.ILightingDataAsset)
		{
			exportCollection = null;
			return false;
		}

		if (asset.MainAsset is SpriteInformationObject spriteInformationObject && (ExportSprites || asset is not ISprite))
		{
			exportCollection = new TextureExportCollection(this, spriteInformationObject, ExportSprites);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}

	public override bool Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		ITexture2D texture = (ITexture2D)asset;
		if (!texture.CheckAssetIntegrity())
		{
			Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{texture.Name}' because resources file '{texture.StreamData_C28?.Path}' hasn't been found");
			return false;
		}

		if (TextureConverter.TryConvertToBitmap(texture, out DirectBitmap bitmap))
		{
			using Stream stream = fileSystem.File.Create(path);
			bitmap.Save(stream, ImageExportFormat);
			return true;
		}
		else
		{
			Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{texture.Name}' to bitmap");
			return false;
		}
	}
}
