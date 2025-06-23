using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.QualitySetting;

namespace AssetRipper.SourceGenerated.Extensions;

public static class QualitySettingExtensions
{
	public enum AntiAliasing
	{
		Disabled = 0,
		_2X = 2,
		_4X = 4,
		_8X = 8,
	}
	public enum ShadowCascades
	{
		NoCascades = 1,
		TwoCascades = 2,
		FourCascades = 4,
	}
	public enum TextureQuality
	{
		FullRes = 0,
		HalfRes = 1,
		QuarterRes = 2,
		EighthRes = 3,
	}
	public enum VSyncCount
	{
		DontSync = 0,
		EveryVBlank = 1,
		EverySecondVBlank = 2,
	}
	public static void ConvertToEditorFormat(this IQualitySetting setting)
	{
		setting.ShadowProjection = (int)ShadowProjection.StableFit;
		setting.ShadowNearPlaneOffset = 3.0f;
		setting.ShadowCascade2Split = 1.0f / 3.0f;
		setting.ShadowCascade4Split?.SetValues(2.0f / 30.0f, 0.2f, 14.0f / 30.0f);
		setting.StreamingMipmapsAddAllCameras = true;
		setting.StreamingMipmapsMemoryBudget = 512.0f;
		setting.StreamingMipmapsRenderersPerFrame = 512;
		setting.StreamingMipmapsMaxLevelReduction = 2;
		setting.StreamingMipmapsMaxFileIORequests = 1024;
		setting.AsyncUploadTimeSlice = 2;
		setting.AsyncUploadBufferSize = 4;
		setting.ResolutionScalingFixedDPIFactor = 1.0f;
	}

	public static ShadowQuality GetShadows(this IQualitySetting setting)
	{
		return (ShadowQuality)setting.Shadows;
	}

	public static ShadowResolution GetShadowResolution(this IQualitySetting setting)
	{
		return (ShadowResolution)setting.ShadowResolution;
	}

	public static ShadowProjection Get(this IQualitySetting setting)
	{
		return (ShadowProjection)setting.ShadowProjection;
	}

	public static ShadowCascades GetShadowCascades(this IQualitySetting setting)
	{
		return (ShadowCascades)setting.ShadowCascades;
	}

	public static ShadowmaskMode GetShadowmaskMode(this IQualitySetting setting)
	{
		return (ShadowmaskMode)setting.ShadowmaskMode;
	}

	public static SkinWeights GetSkinWeights(this IQualitySetting setting)
	{
		return (SkinWeights)(setting.Has_SkinWeights() ? setting.SkinWeights : setting.BlendWeights);
		//todo: merge BlendWeights into SkinWeights in the source generation
	}

	public static TextureQuality GetTextureQuality(this IQualitySetting setting)
	{
		return (TextureQuality)setting.TextureQuality;
	}

	public static AnisotropicFiltering GetAnisotropicTextures(this IQualitySetting setting)
	{
		return (AnisotropicFiltering)setting.AnisotropicTextures;
	}

	public static AntiAliasing GetAntiAliasing(this IQualitySetting setting)
	{
		return (AntiAliasing)setting.AntiAliasing;
	}

	public static VSyncCount GetVSyncCount(this IQualitySetting setting)
	{
		return (VSyncCount)setting.VSyncCount;
	}
}
