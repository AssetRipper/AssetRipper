using System;
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

	/// <summary>
	/// Applies standard Editor default values to properties that might be missing, 
	/// incomplete, or stripped in the serialized release build format.
	/// </summary>
	public static void ConvertToEditorFormat(this IQualitySetting setting)
	{
		ArgumentNullException.ThrowIfNull(setting);

		// Core Shadows
		setting.ShadowProjection = (int)ShadowProjection.StableFit;
		setting.ShadowNearPlaneOffset = 3.0f;
		setting.ShadowCascade2Split = 1.0f / 3.0f;
		
		// Guarded with null-conditional because Vector structs map to nullable reference types in AR's generation.
		setting.ShadowCascade4Split?.SetValues(2.0f / 30.0f, 0.2f, 14.0f / 30.0f);
		
		// Streaming Mipmaps (Introduced in Unity 2018.2)
		setting.StreamingMipmapsAddAllCameras = true;
		setting.StreamingMipmapsMemoryBudget = 512.0f;
		setting.StreamingMipmapsRenderersPerFrame = 512;
		setting.StreamingMipmapsMaxLevelReduction = 2;
		setting.StreamingMipmapsMaxFileIORequests = 1024;
		
		// Async Upload Settings (Introduced in Unity 2018.3)
		setting.AsyncUploadTimeSlice = 2;
		setting.AsyncUploadBufferSize = 4;
		
		// Resolution Scaling (Introduced in Unity 2017.1)
		setting.ResolutionScalingFixedDPIFactor = 1.0f;
	}

	public static ShadowQuality GetShadows(this IQualitySetting setting)
	{
		ArgumentNullException.ThrowIfNull(setting);
		return (ShadowQuality)setting.Shadows;
	}

	public static ShadowResolution GetShadowResolution(this IQualitySetting setting)
	{
		ArgumentNullException.ThrowIfNull(setting);
		return (ShadowResolution)setting.ShadowResolution;
	}

	public static ShadowProjection GetShadowProjection(this IQualitySetting setting)
	{
		ArgumentNullException.ThrowIfNull(setting);
		return (ShadowProjection)setting.ShadowProjection;
	}

	public static ShadowCascades GetShadowCascades(this IQualitySetting setting)
	{
		ArgumentNullException.ThrowIfNull(setting);
		return (ShadowCascades)setting.ShadowCascades;
	}

	public static ShadowmaskMode GetShadowmaskMode(this IQualitySetting setting)
	{
		ArgumentNullException.ThrowIfNull(setting);
		return (ShadowmaskMode)setting.ShadowmaskMode;
	}

	public static SkinWeights GetSkinWeights(this IQualitySetting setting)
	{
		ArgumentNullException.ThrowIfNull(setting);
		
		// Handles the rename in Unity 2019.1 (BlendWeights -> SkinWeights)
		return (SkinWeights)(setting.Has_SkinWeights() ? setting.SkinWeights : setting.BlendWeights);
		//todo: merge BlendWeights into SkinWeights in the source generation
	}

	public static TextureQuality GetTextureQuality(this IQualitySetting setting)
	{
		ArgumentNullException.ThrowIfNull(setting);
		return (TextureQuality)setting.TextureQuality;
	}

	public static AnisotropicFiltering GetAnisotropicTextures(this IQualitySetting setting)
	{
		ArgumentNullException.ThrowIfNull(setting);
		return (AnisotropicFiltering)setting.AnisotropicTextures;
	}

	public static AntiAliasing GetAntiAliasing(this IQualitySetting setting)
	{
		ArgumentNullException.ThrowIfNull(setting);
		return (AntiAliasing)setting.AntiAliasing;
	}

	public static VSyncCount GetVSyncCount(this IQualitySetting setting)
	{
		ArgumentNullException.ThrowIfNull(setting);
		return (VSyncCount)setting.VSyncCount;
	}
}
