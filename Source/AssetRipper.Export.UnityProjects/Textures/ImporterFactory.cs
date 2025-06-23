using AssetRipper.Assets;
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

namespace AssetRipper.Export.UnityProjects.Textures;

public static class ImporterFactory
{
	public static ITextureImporter GenerateTextureImporter(IExportContainer container, IUnityObjectBase origin)
	{
		TextureImporterData data = new TextureImporterData(origin);
		ITextureImporter instance = TextureImporter.Create(container.File, container.ExportVersion);
		instance.MipMaps.EnableMipMap = data.EnableMipMap ? 1 : 0;
		instance.MipMaps.SRGBTexture = data.SRGBTexture ? 1 : 0;
		instance.MipMaps.AlphaTestReferenceValue = 0.5f;
		instance.MipMaps.MipMapFadeDistanceStart = 1;
		instance.MipMaps.MipMapFadeDistanceEnd = 3;
		instance.BumpMap.HeightScale = .25f;
		instance.GenerateCubemapE = TextureImporterGenerateCubemap.AutoCubemap;
		instance.StreamingMipmaps = data.StreamingMipmaps ? 1 : 0;
		instance.StreamingMipmapsPriority = data.StreamingMipmapsPriority;
		instance.IsReadable = data.IsReadable ? 1 : 0;
		instance.Format = (int)data.Format;
		instance.MaxTextureSize = data.MaxTextureSize;
		instance.TextureSettings.CopyValues(data.TextureSettings);
		// cubemaps break when they aren't scaled, while sprites break if they ARE scaled
		// everything else works with no scaling, so we just only scale for cubemaps
		instance.NPOTScaleE = origin is ICubemap
			? TextureImporterNPOTScale.ToNearest
			: TextureImporterNPOTScale.None;
		instance.CompressionQuality = 50;

		instance.SetSwizzle(TextureImporterSwizzle.R, TextureImporterSwizzle.G, TextureImporterSwizzle.B, TextureImporterSwizzle.A);

		instance.SpriteModeE = SpriteImportMode.Single;
		instance.SpriteExtrude = 1;
		instance.SpriteMeshTypeE = SpriteMeshType.Tight;
		instance.SpritePivot?.SetValues(0.5f, 0.5f);
		instance.SpritePixelsToUnits = 100.0f;
		instance.SpriteGenerateFallbackPhysicsShape = 1;
		instance.AlphaUsageE = TextureImporterAlphaSource.FromInput;
		instance.AlphaIsTransparency = 1;
		instance.SpriteTessellationDetail = -1;
		instance.TextureTypeE = data.TextureType;
		instance.TextureShapeE = data.TextureShape;

		ITextureImporterPlatformSettings platformSettings = instance.PlatformSettings.AddNew();
		platformSettings.BuildTarget = "DefaultTexturePlatform";
		platformSettings.MaxTextureSize = instance.MaxTextureSize;
		platformSettings.ResizeAlgorithm = (int)TextureResizeAlgorithm.Mitchell;
		platformSettings.Format = (int)TextureImporterFormat.Automatic;
		platformSettings.TextureCompression = (int)TextureImporterCompression.CompressedHQ;//Uncompressed results in a significantly larger Library folder
		platformSettings.CompressionQuality = 50;
		platformSettings.CrunchedCompression = false;
		platformSettings.AllowsAlphaSplitting = false;
		platformSettings.Overridden = false;
		platformSettings.AndroidETC2FallbackOverride = (int)AndroidETC2FallbackOverride.UseBuildSettings;
		platformSettings.ForceMaximumCompressionQuality_BC6H_BC7 = false;

		if (instance.Has_AssetBundleName_R() && origin.AssetBundleName is not null)
		{
			instance.AssetBundleName_R = origin.AssetBundleName;
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
						EnableMipMap = texture2DArray.MipCount > 1;
						SRGBTexture = texture2DArray.ColorSpaceE == ColorSpace.Linear;
						StreamingMipmaps = false;
						StreamingMipmapsPriority = default;
						IsReadable = texture2DArray.IsReadable;
						Format = texture2DArray.FormatE;
						MaxTextureSize = CalculateMaxTextureSize(texture2DArray.Width, texture2DArray.Height);
						TextureSettings = texture2DArray.TextureSettings;
						TextureType = texture2DArray.Has_UsageMode() && texture2DArray.UsageModeE.IsNormalmap()
							? TextureImporterType.NormalMap
							: TextureImporterType.Default;
						TextureShape = TextureImporterShape.Texture2DArray;
					}
					break;
				case ICubemapArray cubemapArray:
					{
						EnableMipMap = cubemapArray.MipCount > 1;
						SRGBTexture = cubemapArray.ColorSpaceE == ColorSpace.Linear;
						StreamingMipmaps = false;
						StreamingMipmapsPriority = default;
						IsReadable = cubemapArray.IsReadable;
						Format = cubemapArray.FormatE;
						MaxTextureSize = CalculateMaxTextureSize(cubemapArray.Width, cubemapArray.Width);
						TextureSettings = cubemapArray.TextureSettings;
						TextureType = cubemapArray.Has_UsageMode() && cubemapArray.UsageModeE.IsNormalmap()
							? TextureImporterType.NormalMap
							: TextureImporterType.Default;
						TextureShape = TextureImporterShape.Texture2DArray;//Maybe this should be TextureCube
					}
					break;
				case ITexture3D texture3D:
					{
						EnableMipMap = texture3D.Has_MipCount() && texture3D.MipCount > 1 || texture3D.Has_MipMap() && texture3D.MipMap;
						SRGBTexture = texture3D.ColorSpaceE == ColorSpace.Linear;
						StreamingMipmaps = false;
						StreamingMipmapsPriority = default;
						IsReadable = texture3D.IsReadable;
						Format = texture3D.GetTextureFormat();
						MaxTextureSize = CalculateMaxTextureSize(texture3D.Width, texture3D.Height);
						TextureSettings = texture3D.TextureSettings;
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
		if (container.ExportVersion.LessThan(5, 6))
		{
			Logger.Warning("IHVImageFormatImporter doesn't exist on versions less than 5.6. A different importer needs to be used on this version");
		}
		IIHVImageFormatImporter instance = IHVImageFormatImporter.Create(container.File, container.ExportVersion);
		instance.SetToDefault();
		instance.IsReadable = origin.IsReadable_C28;
		instance.SRGBTexture = origin.ColorSpace_C28E == ColorSpace.Linear;
		instance.StreamingMipmaps = origin.StreamingMipmaps_C28;
		instance.StreamingMipmapsPriority = origin.StreamingMipmapsPriority_C28;
		if (origin.AssetBundleName is not null)
		{
			instance.AssetBundleName_R = origin.AssetBundleName;
		}
		return instance;
	}
}
