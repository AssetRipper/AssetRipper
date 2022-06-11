using AssetRipper.Core.Classes.Meta.Importers.Texture;
using AssetRipper.Core.Classes.Sprite;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_1006;
using AssetRipper.SourceGenerated.Classes.ClassID_1055;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_89;
using AssetRipper.SourceGenerated.Subclasses.TextureImporterPlatformSettings;

namespace AssetRipper.Library.Exporters.Textures
{
	public static class ImporterFactory
	{
		public static ITextureImporter GenerateTextureImporter(IExportContainer container, ITexture2D origin)
		{
			ITextureImporter instance = TextureImporterFactory.CreateAsset(container.ExportLayout.Version);
			instance.MipMaps_C1006.EnableMipMap = origin.MipCount_C28 > 1 ? 1 : 0;
			instance.MipMaps_C1006.SRGBTexture = origin.ColorSpace_C28 == (int)ColorSpace.Linear ? 1 : 0;
			instance.MipMaps_C1006.AlphaTestReferenceValue = 0.5f;
			instance.MipMaps_C1006.MipMapFadeDistanceStart = 1;
			instance.MipMaps_C1006.MipMapFadeDistanceEnd = 3;
			instance.BumpMap_C1006.HeightScale = .25f;
			instance.GenerateCubemap_C1006 = (int)TextureImporterGenerateCubemap.AutoCubemap;
			instance.StreamingMipmaps_C1006 = origin.StreamingMipmaps_C28 ? 1 : 0;
			instance.StreamingMipmapsPriority_C1006 = origin.StreamingMipmapsPriority_C28;
			instance.IsReadable_C1006 = origin.IsReadable_C28 ? 1 : 0;
			instance.TextureFormat_C1006 = origin.TextureFormat_C28;
			instance.MaxTextureSize_C1006 = Math.Min(2048, Math.Max(origin.Width_C28, origin.Height_C28));
			instance.TextureSettings_C1006.CopyValues(origin.TextureSettings_C28);
			instance.NPOTScale_C1006 = (int)TextureImporterNPOTScale.None;
			instance.CompressionQuality_C1006 = 50;

			instance.SpriteMode_C1006 = (int)SpriteImportMode.Single;
			instance.SpriteExtrude_C1006 = 1;
			instance.SpriteMeshType_C1006 = (int)SpriteMeshType.Tight;
			instance.SpritePivot_C1006?.SetValues(0.5f, 0.5f);
			instance.SpritePixelsToUnits_C1006 = 100.0f;
			instance.SpriteGenerateFallbackPhysicsShape_C1006 = 1;
			instance.AlphaUsage_C1006 = (int)TextureImporterAlphaSource.FromInput;
			instance.AlphaIsTransparency_C1006 = 1;
			instance.SpriteTessellationDetail_C1006 = -1;
			instance.TextureType_C1006 = GetTextureTypeFromLightmapFormat(origin);
			instance.TextureShape_C1006 = origin is ICubemap
				? (int)TextureImporterShape.TextureCube
				: (int)TextureImporterShape.Texture2D;

			ITextureImporterPlatformSettings platformSettings = instance.PlatformSettings_C1006.AddNew();
			platformSettings.BuildTarget.String = "DefaultTexturePlatform";
			platformSettings.MaxTextureSize = 2048;
			platformSettings.ResizeAlgorithm = (int)TextureResizeAlgorithm.Mitchell;
			platformSettings.TextureFormat = (int)TextureFormat.Automatic;
			platformSettings.TextureCompression = (int)TextureImporterCompression.Uncompressed;
			platformSettings.CompressionQuality = 50;
			platformSettings.CrunchedCompression = false;
			platformSettings.AllowsAlphaSplitting = false;
			platformSettings.Overridden = false;
			platformSettings.AndroidETC2FallbackOverride = (int)AndroidETC2FallbackOverride.UseBuildSettings;
			platformSettings.ForceMaximumCompressionQuality_BC6H_BC7 = false;

			return instance;
		}

		private static int GetTextureTypeFromLightmapFormat(ITexture2D origin)
		{
			return ((TextureUsageMode)origin.LightmapFormat_C28).IsNormalmap()
				? (int)TextureImporterType.NormalMap
				: (int)TextureImporterType.Default;
		}

		public static IIHVImageFormatImporter GenerateIHVImporter(IExportContainer container, ITexture2D origin)
		{
			if (container.ExportLayout.Version.IsLess(5, 6))
			{
				Logger.Warning("IHVImageFormatImporter doesn't exist on versions less than 5.6. A different importer needs to be used on this version");
			}
			IIHVImageFormatImporter instance = IHVImageFormatImporterFactory.CreateAsset(container.ExportLayout.Version);
			instance.SetToDefault();
			instance.IsReadable_C1055 = origin.IsReadable_C28;
			instance.SRGBTexture_C1055 = origin.ColorSpace_C28 == (int)ColorSpace.Linear;
			instance.StreamingMipmaps_C1055 = origin.StreamingMipmaps_C28;
			instance.StreamingMipmapsPriority_C1055 = origin.StreamingMipmapsPriority_C28;
			return instance;
		}
	}
}
