using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.NativeEnums.Global;
using AssetRipper.SourceGenerated.Subclasses.LightmapEditorSettings;
using FilterMode = AssetRipper.SourceGenerated.Enums.FilterMode_2;
using Lightmapper = AssetRipper.SourceGenerated.Enums.Lightmapper_0;
using LightmapsMode = AssetRipper.SourceGenerated.Enums.LightmapsMode;
using MixedLightingMode = AssetRipper.SourceGenerated.Enums.MixedLightingMode;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class LightmapEditorSettingsExtensions
	{
		public static void SetToDefault(this ILightmapEditorSettings settings)
		{
			settings.Resolution = 2.0f;
			settings.BakeResolution = 40.0f;
			settings.TextureWidth = 1024;
			settings.TextureHeight = 1024;
			settings.AO = false;
			settings.AOMaxDistance = 1.0f;
			settings.CompAOExponent = 1.0f;
			settings.CompAOExponentDirect = 0.0f;
			settings.ExtractAmbientOcclusion = false;
			settings.Padding = 2;
			settings.LightmapsBakeMode = (int)LightmapsMode.CombinedDirectional;
			settings.TextureCompression = true;
			settings.FinalGather = false;
			settings.FinalGatherFiltering = true;
			settings.FinalGatherRayCount = 256;
			settings.ReflectionCompression = (int)ReflectionCubemapCompression.Auto;
			settings.MixedBakeMode = (int)MixedLightingMode.Shadowmask;
			settings.BakeBackend = (int)Lightmapper.Enlighten;
			settings.PVRSampling = (int)Sampling.Fixed;
			settings.PVRDirectSampleCount = 32;
			settings.PVRSampleCount = 500;
			settings.PVRBounces = 2;
			settings.PVREnvironmentSampleCount = 256;
			settings.PVREnvironmentReferencePointCount = 2048;
			settings.PVRFilteringMode = (int)FilterMode.Auto;
			settings.PVRDenoiserTypeDirect = (int)DenoiserType.Optix;
			settings.PVRDenoiserTypeIndirect = (int)DenoiserType.Optix;
			settings.PVRDenoiserTypeAO = (int)DenoiserType.Optix;
			settings.PVRFilterTypeDirect = (int)FilterType.Gaussian;
			settings.PVRFilterTypeIndirect = (int)FilterType.Gaussian;
			settings.PVRFilterTypeAO = (int)FilterType.Gaussian;
			settings.PVREnvironmentMIS = 1;
			settings.PVRCulling = true;
			settings.PVRFilteringGaussRadiusDirect = 1;
			settings.PVRFilteringGaussRadiusIndirect = 5;
			settings.PVRFilteringGaussRadiusAO = 2;
			settings.PVRFilteringAtrousPositionSigmaDirect = 0.5f;
			settings.PVRFilteringAtrousPositionSigmaIndirect = 2.0f;
			settings.PVRFilteringAtrousPositionSigmaAO = 1.0f;
			settings.ShowResolutionOverlay = true;
			settings.ExportTrainingData = false;
			if (settings.Has_TrainingDataDestination())
			{
				settings.TrainingDataDestination = "TrainingData";
			}
			settings.LightProbeSampleCountMultiplier = 4.0f;
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
}
