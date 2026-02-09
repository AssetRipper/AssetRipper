using AssetRipper.SourceGenerated.Classes.ClassID_850595691;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.LightmapEditorSettings;
using FilterMode = AssetRipper.SourceGenerated.Enums.FilterMode_2;
using Lightmapper = AssetRipper.SourceGenerated.Enums.Lightmapper_0;
using LightmapsMode = AssetRipper.SourceGenerated.Enums.LightmapsMode;
using MixedLightingMode = AssetRipper.SourceGenerated.Enums.MixedLightingMode;

namespace AssetRipper.SourceGenerated.Extensions;

public static class LightmapEditorSettingsExtensions
{
	public static void SetToDefault(this ILightmapEditorSettings settings)
	{
		settings.AO = false;
		settings.AOAmount = 0;
		settings.AOContrast = 1f;
		settings.AOMaxDistance = 1f;
		settings.AtlasSize = 1024;
		settings.BakeBackend = (int)Lightmapper.Enlighten;
		settings.BakeResolution = 40f;
		settings.BounceBoost = 1f;
		settings.BounceIntensity = 1f;
		settings.Bounces = 1;
		settings.CompAOExponent = 1f;
		settings.CompAOExponentDirect = 0f;
		settings.DirectLightInLightProbes = true;
		settings.ExportTrainingData = false;
		settings.ExtractAmbientOcclusion = false;
		settings.FinalGather = false;
		settings.FinalGatherContrastThreshold = 0.05f;
		settings.FinalGatherFiltering = true;
		settings.FinalGatherGradientThreshold = 0f;
		settings.FinalGatherInterpolationPoints = 15;
		settings.FinalGatherRayCount = 256;
		settings.FinalGatherRays = 1000;
		settings.LastUsedResolution = 0f;
		settings.LightmapsBakeMode = (int)LightmapsMode.CombinedDirectional;
		settings.LightProbeSampleCountMultiplier = 4f;
		settings.LockAtlas = false;
		settings.LODSurfaceMappingDistance = 1f;
		settings.MixedBakeMode = (int)MixedLightingMode.Shadowmask;
		settings.Padding = 2;
		settings.PVRBounces = 2;
		settings.PVRCulling = true;
		settings.PVRDenoiserTypeAO = (int)DenoiserType.Optix;
		settings.PVRDenoiserTypeDirect = (int)DenoiserType.Optix;
		settings.PVRDenoiserTypeIndirect = (int)DenoiserType.Optix;
		settings.PVRDirectSampleCount = 32;
		settings.PVREnvironmentMIS = 1;
		settings.PVREnvironmentSampleCount = 256;
		settings.PVREnvironmentReferencePointCount = 2048;
		settings.PVRFiltering = 0;
		settings.PVRFilteringAtrousColorSigma = 1f;
		settings.PVRFilteringAtrousNormalSigma = 1f;
		settings.PVRFilteringAtrousPositionSigma = 1f;
		settings.PVRFilteringAtrousPositionSigmaAO = 1f;
		settings.PVRFilteringAtrousPositionSigmaDirect = 0.5f;
		settings.PVRFilteringAtrousPositionSigmaIndirect = 2f;
		settings.PVRFilteringGaussRadiusAO = 2;
		settings.PVRFilteringGaussRadiusDirect = 1;
		settings.PVRFilteringGaussRadiusIndirect = 5;
		settings.PVRFilteringMode = (int)FilterMode.Auto;
		settings.PVRFilterTypeAO = (int)FilterType.Gaussian;
		settings.PVRFilterTypeDirect = (int)FilterType.Gaussian;
		settings.PVRFilterTypeIndirect = (int)FilterType.Gaussian;
		settings.PVRSampleCount = 500;
		settings.PVRSampling = (int)Sampling.Fixed;
		settings.Quality = 0;
		settings.ReflectionCompression = (int)ReflectionCubemapCompression.Auto;
		settings.Resolution = 2f;
		settings.ShowResolutionOverlay = true;
		settings.SkyLightColor?.SetValues(.86f, .93f, 1f, 1f);
		settings.SkyLightIntensity = 0f;
		settings.StationaryBakeMode = 1;
		settings.TextureCompression = true;
		settings.TextureHeight = 1024;
		settings.TextureWidth = 1024;
		if (settings.Has_TrainingDataDestination())
		{
			settings.TrainingDataDestination = "TrainingData";
		}
	}

	public static void ConvertToEditorFormat(this ILightingSettings settings)
	{
		settings.AO = false;
		settings.AOMaxDistance = 1f;
		settings.BakeBackend = (int)Lightmapper.Enlighten;
		settings.BakeResolution = 40f;
		settings.CompAOExponent = 1f;
		settings.CompAOExponentDirect = 0f;
		settings.DisableWorkerProcessBaking = false;
		settings.ExportTrainingData = false;
		settings.ExtractAO = false;
		settings.FilterMode = (int)FilterMode.Auto;
		settings.FinalGather = false;
		settings.FinalGatherFiltering = true;
		settings.FinalGatherRayCount = 256;
		settings.ForceUpdates = false;
		settings.ForceWhiteAlbedo = false;
		settings.LightmapCompressionE = LightmapCompression.HighQuality;
		settings.LightmapMaxSize = 4096;
		settings.LightmapsBakeMode = (int)LightmapsMode.CombinedDirectional;
		settings.LightmapSizeFixed = false;
		settings.LightProbeSampleCountMultiplier = 4f;
		settings.MixedBakeMode = (int)MixedLightingMode.Shadowmask;
		settings.NumRaysToShootPerTexel = -1;
		settings.Padding = 2;
		settings.PVRBounces = 2;
		settings.PVRCulling = true;
		settings.PVRDenoiserTypeAO = (int)DenoiserType.Optix;
		settings.PVRDenoiserTypeDirect = (int)DenoiserType.Optix;
		settings.PVRDenoiserTypeIndirect = (int)DenoiserType.Optix;
		settings.PVRDirectSampleCount = 32;
		settings.PVREnvironmentImportanceSampling = true;
		settings.PVREnvironmentMIS = 1;
		settings.PVREnvironmentReferencePointCount = 2048;
		settings.PVREnvironmentSampleCount = 256;
		settings.PVRFilteringAtrousPositionSigmaAO = 1f;
		settings.PVRFilteringAtrousPositionSigmaDirect = 0.5f;
		settings.PVRFilteringAtrousPositionSigmaIndirect = 2f;
		settings.PVRFilteringGaussRadiusAO_Int32 = 2;
		settings.PVRFilteringGaussRadiusAO_Single = 2f;
		settings.PVRFilteringGaussRadiusDirect_Int32 = 1;
		settings.PVRFilteringGaussRadiusDirect_Single = 1f;
		settings.PVRFilteringGaussRadiusIndirect_Int32 = 5;
		settings.PVRFilteringGaussRadiusIndirect_Single = 5f;
		settings.PVRFilteringMode = (int)FilterMode.Auto;
		settings.PVRFilterTypeAO = (int)FilterType.Gaussian;
		settings.PVRFilterTypeDirect = (int)FilterType.Gaussian;
		settings.PVRFilterTypeIndirect = (int)FilterType.Gaussian;
		settings.PVRMinBounces = 2;
		settings.PVRRussianRouletteStartBounce = 2;
		settings.PVRSampleCount = 500;
		settings.PVRSampling = (int)Sampling.Fixed;
		settings.PVRTiledBaking = 0;
		settings.RealtimeResolution = 2f;
		settings.RespectSceneVisibilityWhenBakingGI = false;
		settings.TextureCompression = true;
		settings.TrainingDataDestination = "TrainingData";
		settings.UseMipmapLimits = true;
	}

	public static LightmapsMode GetLightmapsBakeMode(this ILightmapEditorSettings settings)
	{
		return (LightmapsMode)settings.LightmapsBakeMode;
	}

	public static ReflectionCubemapCompression GetReflectionCompression(this ILightmapEditorSettings settings)
	{
		return (ReflectionCubemapCompression)settings.ReflectionCompression;
	}

	public static MixedLightingMode GetMixedBakeMode(this ILightmapEditorSettings settings)
	{
		if (settings.Has_MixedBakeMode())
		{
			return (MixedLightingMode)settings.MixedBakeMode;
		}
		else
		{
			return (MixedLightingMode)settings.StationaryBakeMode;//need to rename
		}
	}

	public static Lightmapper GetBakeBackend(this ILightmapEditorSettings settings)
	{
		return (Lightmapper)settings.BakeBackend;
	}

	public static Sampling GetPVRSampling(this ILightmapEditorSettings settings)
	{
		return (Sampling)settings.PVRSampling;
	}

	public static FilterMode GetPVRFilteringMode(this ILightmapEditorSettings settings)
	{
		return (FilterMode)settings.PVRFilteringMode;
	}

	public static DenoiserType GetPVRDenoiserTypeDirect(this ILightmapEditorSettings settings)
	{
		return (DenoiserType)settings.PVRDenoiserTypeDirect;
	}

	public static DenoiserType GetPVRDenoiserTypeIndirect(this ILightmapEditorSettings settings)
	{
		return (DenoiserType)settings.PVRDenoiserTypeIndirect;
	}

	public static DenoiserType GetPVRDenoiserTypeAO(this ILightmapEditorSettings settings)
	{
		return (DenoiserType)settings.PVRDenoiserTypeAO;
	}

	public static FilterType GetPVRFilterTypeDirect(this ILightmapEditorSettings settings)
	{
		return (FilterType)settings.PVRFilterTypeDirect;
	}

	public static FilterType GetPVRFilterTypeIndirect(this ILightmapEditorSettings settings)
	{
		return (FilterType)settings.PVRFilterTypeIndirect;
	}

	public static FilterType GetPVRFilterTypeAO(this ILightmapEditorSettings settings)
	{
		return (FilterType)settings.PVRFilterTypeAO;
	}
}
