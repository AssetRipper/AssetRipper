using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_1006;
using AssetRipper.SourceGenerated.Classes.ClassID_1055;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_89;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.GLTextureSettings;
using AssetRipper.SourceGenerated.Subclasses.TextureImporterPlatformSettings;
using System.Numerics;

namespace AssetRipper.Export.UnityProjects.Textures
{
	public static class ImporterFactory
	{
		public static ITextureImporter GenerateTextureImporter(IExportContainer container, IUnityObjectBase origin)
		{
			TextureImporterData data = new TextureImporterData(origin);
			ITextureImporter instance = TextureImporterFactory.CreateAsset(container.File, container.ExportVersion);
			instance.MipMaps_C1006.EnableMipMap = data.EnableMipMap ? 1 : 0;
			instance.MipMaps_C1006.SRGBTexture = data.SRGBTexture ? 1 : 0;
			instance.MipMaps_C1006.AlphaTestReferenceValue = 0.5f;
			instance.MipMaps_C1006.MipMapFadeDistanceStart = 1;
			instance.MipMaps_C1006.MipMapFadeDistanceEnd = 3;
			instance.BumpMap_C1006.HeightScale = .25f;
			instance.GenerateCubemap_C1006E = TextureImporterGenerateCubemap.AutoCubemap;
			instance.StreamingMipmaps_C1006 = data.StreamingMipmaps ? 1 : 0;
			instance.StreamingMipmapsPriority_C1006 = data.StreamingMipmapsPriority;
			instance.IsReadable_C1006 = data.IsReadable ? 1 : 0;
			instance.Format_C1006 = (int)data.Format;
			instance.MaxTextureSize_C1006 = data.MaxTextureSize;
			instance.TextureSettings_C1006.CopyValues(data.TextureSettings);
			instance.NPOTScale_C1006E = TextureImporterNPOTScale.ToNearest; // Default texture importer settings uses this value, and cubemaps appear to not work when it's None
			instance.CompressionQuality_C1006 = 50;

			instance.SetSwizzle(TextureImporterSwizzle.R, TextureImporterSwizzle.G, TextureImporterSwizzle.B, TextureImporterSwizzle.A);

			instance.SpriteMode_C1006E = SpriteImportMode.Single;
			instance.SpriteExtrude_C1006 = 1;
			instance.SpriteMeshType_C1006E = SpriteMeshType.Tight;
			instance.SpritePivot_C1006?.SetValues(0.5f, 0.5f);
			instance.SpritePixelsToUnits_C1006 = 100.0f;
			instance.SpriteGenerateFallbackPhysicsShape_C1006 = 1;
			instance.AlphaUsage_C1006E = TextureImporterAlphaSource.FromInput;
			instance.AlphaIsTransparency_C1006 = 1;
			instance.SpriteTessellationDetail_C1006 = -1;
			instance.TextureType_C1006E = data.TextureType;
			instance.TextureShape_C1006E = data.TextureShape;

			ITextureImporterPlatformSettings platformSettings = instance.PlatformSettings_C1006.AddNew();
			platformSettings.BuildTarget = "DefaultTexturePlatform";
			platformSettings.MaxTextureSize = instance.MaxTextureSize_C1006;
			platformSettings.ResizeAlgorithm = (int)TextureResizeAlgorithm.Mitchell;
			platformSettings.Format = (int)TextureImporterFormat.Automatic;
			platformSettings.TextureCompression = (int)TextureImporterCompression.Compressed;//Uncompressed results in a significantly larger Library folder
			platformSettings.CompressionQuality = 50;
			platformSettings.CrunchedCompression = false;
			platformSettings.AllowsAlphaSplitting = false;
			platformSettings.Overridden = false;
			platformSettings.AndroidETC2FallbackOverride = (int)AndroidETC2FallbackOverride.UseBuildSettings;
			platformSettings.ForceMaximumCompressionQuality_BC6H_BC7 = false;

			if (instance.Has_AssetBundleName_C1006() && origin.AssetBundleName is not null)
			{
				instance.AssetBundleName_C1006 = origin.AssetBundleName;
			}

			return instance;
		}

		private static int CalculateMaxTextureSize(int width, int height)
		{
			uint maxSideLength = (uint)Math.Max(width, height);
			return Math.Max(2048, (int)BitOperations.RoundUpToPowerOf2(maxSideLength));
		}

		private readonly ref struct TextureImporterData
		{
			public bool EnableMipMap { get; }
			public bool SRGBTexture { get; }
			public bool StreamingMipmaps { get; }
			public int StreamingMipmapsPriority { get; }
			public bool IsReadable { get; }
			public TextureFormat Format { get; }
			public int MaxTextureSize { get; }
			public IGLTextureSettings? TextureSettings { get; }
			public TextureImporterType TextureType { get; }
			public TextureImporterShape TextureShape { get; }

			public TextureImporterData(IUnityObjectBase asset)
			{
				switch (asset)
				{
					case ITexture2D texture2D:
						{
							EnableMipMap = texture2D.Has_MipCount_C28() && texture2D.MipCount_C28 > 1 || texture2D.Has_MipMap_C28() && texture2D.MipMap_C28;
							SRGBTexture = texture2D.ColorSpace_C28E == ColorSpace.Linear;
							StreamingMipmaps = texture2D.StreamingMipmaps_C28;
							StreamingMipmapsPriority = texture2D.StreamingMipmapsPriority_C28;
							IsReadable = texture2D.IsReadable_C28;
							Format = texture2D.Format_C28E;
							MaxTextureSize = CalculateMaxTextureSize(texture2D.Width_C28, texture2D.Height_C28);
							TextureSettings = texture2D.TextureSettings_C28;
							TextureType = texture2D.LightmapFormat_C28E.IsNormalmap()
								? TextureImporterType.NormalMap
								: TextureImporterType.Default;
							TextureShape = texture2D is ICubemap
								? TextureImporterShape.TextureCube
								: TextureImporterShape.Texture2D;
						}
						break;
					case ITexture2DArray texture2DArray:
						{
							EnableMipMap = texture2DArray.MipCount_C187 > 1;
							SRGBTexture = texture2DArray.ColorSpace_C187E == ColorSpace.Linear;
							StreamingMipmaps = false;
							StreamingMipmapsPriority = default;
							IsReadable = texture2DArray.IsReadable_C187;
							Format = texture2DArray.Format_C187E;
							MaxTextureSize = CalculateMaxTextureSize(texture2DArray.Width_C187, texture2DArray.Height_C187);
							TextureSettings = texture2DArray.TextureSettings_C187;
							TextureType = texture2DArray.Has_UsageMode_C187() && texture2DArray.UsageMode_C187E.IsNormalmap()
								? TextureImporterType.NormalMap
								: TextureImporterType.Default;
							TextureShape = TextureImporterShape.Texture2DArray;
						}
						break;
					case ICubemapArray cubemapArray:
						{
							EnableMipMap = cubemapArray.MipCount_C188 > 1;
							SRGBTexture = cubemapArray.ColorSpace_C188E == ColorSpace.Linear;
							StreamingMipmaps = false;
							StreamingMipmapsPriority = default;
							IsReadable = cubemapArray.IsReadable_C188;
							Format = cubemapArray.Format_C188E;
							MaxTextureSize = CalculateMaxTextureSize(cubemapArray.Width_C188, cubemapArray.Width_C188);
							TextureSettings = cubemapArray.TextureSettings_C188;
							TextureType = cubemapArray.Has_UsageMode_C188() && cubemapArray.UsageMode_C188E.IsNormalmap()
								? TextureImporterType.NormalMap
								: TextureImporterType.Default;
							TextureShape = TextureImporterShape.Texture2DArray;//Maybe this should be TextureCube
						}
						break;
					case ITexture3D texture3D:
						{
							EnableMipMap = texture3D.Has_MipCount_C117() && texture3D.MipCount_C117 > 1 || texture3D.Has_MipMap_C117() && texture3D.MipMap_C117;
							SRGBTexture = texture3D.ColorSpace_C117E == ColorSpace.Linear;
							StreamingMipmaps = false;
							StreamingMipmapsPriority = default;
							IsReadable = texture3D.IsReadable_C117;
							Format = texture3D.GetTextureFormat();
							MaxTextureSize = CalculateMaxTextureSize(texture3D.Width_C117, texture3D.Height_C117);
							TextureSettings = texture3D.TextureSettings_C117;
							TextureType = texture3D.GetLightmapFormat().IsNormalmap()
								? TextureImporterType.NormalMap
								: TextureImporterType.Default;
							TextureShape = TextureImporterShape.Texture3D;
						}
						break;
					default:
						throw new ArgumentException($"Asset type not supported: {asset.GetType().Name}", nameof(asset));
				}
			}
		}

		public static IIHVImageFormatImporter GenerateIHVImporter(IExportContainer container, ITexture2D origin)
		{
			if (container.ExportVersion.IsLess(5, 6))
			{
				Logger.Warning("IHVImageFormatImporter doesn't exist on versions less than 5.6. A different importer needs to be used on this version");
			}
			IIHVImageFormatImporter instance = IHVImageFormatImporterFactory.CreateAsset(container.File, container.ExportVersion);
			instance.SetToDefault();
			instance.IsReadable_C1055 = origin.IsReadable_C28;
			instance.SRGBTexture_C1055 = origin.ColorSpace_C28E == ColorSpace.Linear;
			instance.StreamingMipmaps_C1055 = origin.StreamingMipmaps_C28;
			instance.StreamingMipmapsPriority_C1055 = origin.StreamingMipmapsPriority_C28;
			if (origin.AssetBundleName is not null)
			{
				instance.AssetBundleName_C1055 = origin.AssetBundleName;
			}
			return instance;
		}
	}
}
