using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Lights;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.LightmapSettingss
{
	/// <summary>
	/// 3.0.0 - first introduction
	/// </summary>
	public struct LightmapEditorSettings : IAsset, IDependent
	{
		public LightmapEditorSettings(Version version):
			this()
		{
#warning TODO:
			Resolution = 2.0f;
			BakeResolution = 40.0f;
			TextureWidth = 1024;
			TextureHeight = 1024;
			AO = false;
			AOMaxDistance = 1.0f;
			CompAOExponent = 1.0f;
			CompAOExponentDirect = 0.0f;
			ExtractAmbientOcclusion = false;
			Padding = 2;
			LightmapParameters = default;
			LightmapsBakeMode = LightmapsMode.CombinedDirectional;
			TextureCompression = true;
			FinalGather = false;
			FinalGatherFiltering = true;
			FinalGatherRayCount = 256;
			ReflectionCompression = ReflectionCubemapCompression.Auto;
			MixedBakeMode = MixedLightingMode.Shadowmask;
			BakeBackend = Lightmapper.Enlighten;
			PVRSampling = Sampling.Fixed;
			PVRDirectSampleCount = 32;
			PVRSampleCount = 500;
			PVRBounces = 2;
			PVREnvironmentSampleCount = 256;
			PVREnvironmentReferencePointCount = 2048;
			PVRFilteringMode = FilterMode.Auto;
			PVRDenoiserTypeDirect = DenoiserType.Optix;
			PVRDenoiserTypeIndirect = DenoiserType.Optix;
			PVRDenoiserTypeAO = DenoiserType.Optix;
			PVRFilterTypeDirect = FilterType.Gaussian;
			PVRFilterTypeIndirect = FilterType.Gaussian;
			PVRFilterTypeAO = FilterType.Gaussian;
			PVREnvironmentMIS = 1;
			PVRCulling = true;
			PVRFilteringGaussRadiusDirect = 1;
			PVRFilteringGaussRadiusIndirect = 5;
			PVRFilteringGaussRadiusAO = 2;
			PVRFilteringAtrousPositionSigmaDirect = 0.5f;
			PVRFilteringAtrousPositionSigmaIndirect = 2.0f;
			PVRFilteringAtrousPositionSigmaAO = 1.0f;
			ShowResolutionOverlay = true;
			ExportTrainingData = false;
			TrainingDataDestination = TrainingDataName;
			LightProbeSampleCountMultiplier = 4.0f;
		}

		public static int ToSerializedVersion(Version version)
		{
			// PVREnvironmentMIS default value has been changed from 0 to 1
			// PVREnvironmentSampleCount is no longer equal to PVRSampleCount
			if (version.IsGreaterEqual(2019, 1, 0, VersionType.Beta))
			{
				return 12;
			}
			// PVRDenoiserTypeDirect default value has been changed from None to Optix
			// PVRDenoiserTypeAO default value has been changed from None to Optix
			// FilterMode.Auto values has been changed:
			//		PVRFilterTypeDirect to Gaussian
			//		PVRFilterTypeAO	to Gaussian
			//		PVRFilteringGaussRadiusDirect to 1
			//		PVRFilteringGaussRadiusIndirect to 5
			//		PVRFilteringGaussRadiusAO to 2
			if (version.IsGreaterEqual(2019))
			{
				return 11;
			}
			// TextureWidth and TextureHeight has been replaced by AtlasSize
			if (version.IsGreaterEqual(2018))
			{
				return 10;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(2017))
			{
				return 9;
			}
			// StationaryBakeMode has been renamed to MixedBakeMode
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 8))
			{
				return 8;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 6))
			{
				return 7;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 2))
			{
				return 6;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5, 4))
			{
				return 4;
			}
			// NOTE: unknown version
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Final))
			{
				return 3;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasBakeResolution(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0bx (NOTE: unknown version)
		/// </summary>
		public static bool HasSystemTexelWidth(Version version) => version.IsEqual(5, 0, 0, VersionType.Beta);
		/// <summary>
		/// 3.0.0 to 5.0.0f1 exclusive (NOTE: unknown version)
		/// </summary>
		public static bool HasLastUsedResolution(Version version) => version.IsGreaterEqual(3) && version.IsLess(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasAtlasSize(Version version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// Less than 5.0.0f1 (NOTE: unknown version)
		/// </summary>
		public static bool HasBounceBoost(Version version) => version.IsLess(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasAO(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// Less than 5.0.0f1
		/// </summary>
		public static bool HasAOAmount(Version version) => version.IsLess(5, 0, 0, VersionType.Final);
		/// <summary>
		/// Less than 5.0.0f1 (NOTE: unknown version)
		/// </summary>
		public static bool HasAOContrast(Version version) => version.IsLess(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasCompAOExponent(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasCompAOExponentDirect(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasExtractAmbientOcclusion(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 3.5.0 to 5.0.0f1 exclusive (NOTE: unknown version)
		/// </summary>
		public static bool HasLODSurfaceMappingDistance(Version version) => version.IsLess(5, 0, 0, VersionType.Final) && version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasPadding(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 5.0.0bx (NOTE: unknown version)
		/// </summary>
		public static bool HasCompDirectScale(Version version) => version.IsEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasLightmapParameters(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasLightmapsBakeMode(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// Less than 5.0.0f1 (NOTE: unknown version)
		/// </summary>
		public static bool HasLockAtlas(Version version) => version.IsLess(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.4.0 to 5.6.0b1
		/// </summary>
		public static bool HasDirectLightInLightProbes(Version version) => version.IsGreaterEqual(5, 4) && version.IsLessEqual(5, 6, 0, VersionType.Beta, 1);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasFinalGather(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasFinalGatherFiltering(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasFinalGatherRayCount(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasReflectionCompression(Version version) => version.IsGreaterEqual(5, 2);
		/// <summary>
		/// 5.6.0b2 and greater
		/// </summary>
		public static bool HasMixedBakeMode(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 2);
		/// <summary>
		/// 5.6.0b6 and greater
		/// </summary>
		public static bool HasBakeBackend(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 6);
		/// <summary>
		/// 5.6.0b10 and greater
		/// </summary>
		public static bool HasPVRDirectSampleCount(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 10);
		/// <summary>
		/// 5.6.0b6 and greater
		/// </summary>
		public static bool HasPVRSampleCount(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 6);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasPVREnvironmentSampleCount(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 5.6.0b6 to 5.6.4 or 2017.1.0 to 2017.1.2 exclusive
		/// </summary>
		public static bool HasPVRFiltering(Version version)
		{
			if (version.IsLess(2017, 1, 2))
			{
				if (version.IsLess(5, 6, 5))
				{
					return version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 6);
				}
				return version.IsGreaterEqual(2017);
			}
			return false;
		}
		/// <summary>
		/// 5.6.5 to 2017.1 exclusive or 2017.1.2 and greater
		/// </summary>
		public static bool HasPVRFilterTypeDirect(Version version)
		{
			if (version.IsGreaterEqual(2017, 1, 2))
			{
				return true;
			}
			if (version.IsGreaterEqual(5, 6, 5))
			{
				return version.IsLess(2017);
			}
			return false;
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasPVREnvironmentMIS(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 5.6.0b6 and greater
		/// </summary>
		public static bool HasPVRFilteringMode(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 6);
		/// <summary>
		/// 5.6.0b6 and greater
		/// </summary>
		public static bool HasPVRCulling(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 6);
		/// <summary>
		/// 5.6.0b6 to 5.6.4 or 2017.1.0 to 2017.1.2 exclusive
		/// </summary>
		public static bool HasPVRFilteringAtrousColorSigma(Version version)
		{
			if (version.IsLess(2017, 1, 2))
			{
				if (version.IsLess(5, 6, 5))
				{
					return version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 6);
				}
				return version.IsGreaterEqual(2017);
			}
			return false;
		}
		/// <summary>
		/// 5.6.5 to 2017.1 exclusive or 2017.1.2 and greater
		/// </summary>
		public static bool HasPVRFilteringAtrousPositionSigmaDirect(Version version)
		{
			if (version.IsGreaterEqual(2017, 1, 2))
			{
				return true;
			}
			if (version.IsGreaterEqual(5, 6, 5))
			{
				return version.IsLess(2017);
			}
			return false;
		}
		/// <summary>
		/// 2017.2.1 to 2019.2 exclusive
		/// </summary>
		public static bool HasShowResolutionOverlay(Version version) => version.IsGreaterEqual(2017, 2, 1) && version.IsLess(2019, 2);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasExportTrainingData(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 2019.2 and greater
		/// </summary>
		public static bool HasTrainingDataDestination(Version version) => version.IsGreaterEqual(2019, 2);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasLightProbeSampleCountMultiplier(Version version) => version.IsGreaterEqual(2019, 3);

		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool CompAOExponentFirst(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsAlign1(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 2017.2.1 and greater
		/// </summary>
		private static bool IsAlign2(Version version) => version.IsGreaterEqual(2017, 2, 1);

		public void Read(AssetReader reader)
		{
			Resolution = reader.ReadSingle();
			if (HasBakeResolution(reader.Version))
			{
				BakeResolution = reader.ReadSingle();
			}
			if (HasSystemTexelWidth(reader.Version))
			{
				SystemTexelWidth = reader.ReadInt32();
			}
			if (HasLastUsedResolution(reader.Version))
			{
				LastUsedResolution = reader.ReadSingle();
			}
			if (HasAtlasSize(reader.Version))
			{
				AtlasSize = reader.ReadInt32();
			}
			else
			{
				TextureWidth = reader.ReadInt32();
				TextureHeight = reader.ReadInt32();
			}

			if (HasBounceBoost(reader.Version))
			{
				BounceBoost = reader.ReadSingle();
				BounceIntensity = reader.ReadSingle();
				SkyLightColor.Read(reader);
				SkyLightIntensity = reader.ReadSingle();
				Quality = reader.ReadInt32();
				Bounces = reader.ReadInt32();
				FinalGatherRays = reader.ReadInt32();
				FinalGatherContrastThreshold = reader.ReadSingle();
				FinalGatherGradientThreshold = reader.ReadSingle();
				FinalGatherInterpolationPoints = reader.ReadInt32();
			}

			if (HasAO(reader.Version))
			{
				AO = reader.ReadBoolean();
				reader.AlignStream();
			}

			if (HasAOAmount(reader.Version))
			{
				AOAmount = reader.ReadSingle();
			}

			AOMaxDistance = reader.ReadSingle();
			if (HasAOContrast(reader.Version))
			{
				AOContrast = reader.ReadSingle();
			}
			if (HasCompAOExponent(reader.Version) && CompAOExponentFirst(reader.Version))
			{
				CompAOExponent = reader.ReadSingle();
			}
			if (HasCompAOExponentDirect(reader.Version))
			{
				CompAOExponentDirect = reader.ReadSingle();
			}
			if (HasExtractAmbientOcclusion(reader.Version))
			{
				ExtractAmbientOcclusion = reader.ReadBoolean();
				reader.AlignStream();
			}
			if (HasLODSurfaceMappingDistance(reader.Version))
			{
				LODSurfaceMappingDistance = reader.ReadSingle();
			}
			if (HasPadding(reader.Version))
			{
				Padding = reader.ReadInt32();
			}
			if (HasCompDirectScale(reader.Version))
			{
				CompDirectScale = reader.ReadSingle();
				CompIndirectScale = reader.ReadSingle();
			}
			if (HasCompAOExponent(reader.Version) && !CompAOExponentFirst(reader.Version))
			{
				CompAOExponent = reader.ReadSingle();
			}
			if (HasLightmapParameters(reader.Version))
			{
				LightmapParameters.Read(reader);
			}
			if (HasLightmapsBakeMode(reader.Version))
			{
				LightmapsBakeMode = (LightmapsMode)reader.ReadInt32();
			}

			TextureCompression = reader.ReadBoolean();
			if (HasLockAtlas(reader.Version))
			{
				LockAtlas = reader.ReadBoolean();
			}
			if (HasDirectLightInLightProbes(reader.Version))
			{
				DirectLightInLightProbes = reader.ReadBoolean();
			}
			if (HasFinalGather(reader.Version))
			{
				FinalGather = reader.ReadBoolean();
			}
			if (HasFinalGatherFiltering(reader.Version))
			{
				FinalGatherFiltering = reader.ReadBoolean();
			}
			if (IsAlign1(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasFinalGatherRayCount(reader.Version))
			{
				FinalGatherRayCount = reader.ReadInt32();
			}
			if (HasReflectionCompression(reader.Version))
			{
				ReflectionCompression = (ReflectionCubemapCompression)reader.ReadInt32();
			}
			if (HasMixedBakeMode(reader.Version))
			{
				MixedBakeMode = (MixedLightingMode)reader.ReadInt32();
			}
			if (HasBakeBackend(reader.Version))
			{
				BakeBackend = (Lightmapper)reader.ReadInt32();
				PVRSampling = (Sampling)reader.ReadInt32();
			}
			if (HasPVRDirectSampleCount(reader.Version))
			{
				PVRDirectSampleCount = reader.ReadInt32();
			}
			if (HasPVRSampleCount(reader.Version))
			{
				PVRSampleCount = reader.ReadInt32();
				PVRBounces = reader.ReadInt32();
			}
			if (HasPVREnvironmentSampleCount(reader.Version))
			{
				PVREnvironmentSampleCount = reader.ReadInt32();
				PVREnvironmentReferencePointCount = reader.ReadInt32();
				PVRFilteringMode = (FilterMode)reader.ReadInt32();
				PVRDenoiserTypeDirect = (DenoiserType)reader.ReadInt32();
				PVRDenoiserTypeIndirect = (DenoiserType)reader.ReadInt32();
				PVRDenoiserTypeAO = (DenoiserType)reader.ReadInt32();
			}
			if (HasPVRFiltering(reader.Version))
			{
				PVRFiltering = reader.ReadInt32();
			}
			else if (HasPVRFilterTypeDirect(reader.Version))
			{
				PVRFilterTypeDirect = (FilterType)reader.ReadInt32();
				PVRFilterTypeIndirect = (FilterType)reader.ReadInt32();
				PVRFilterTypeAO = (FilterType)reader.ReadInt32();
			}
			if (HasPVREnvironmentMIS(reader.Version))
			{
				PVREnvironmentMIS = reader.ReadInt32();
			}
			if (HasPVRFilteringMode(reader.Version) && !HasPVREnvironmentSampleCount(reader.Version))
			{
				PVRFilteringMode = (FilterMode)reader.ReadInt32();
			}
			if (HasPVRCulling(reader.Version))
			{
				PVRCulling = reader.ReadBoolean();
				reader.AlignStream();

				PVRFilteringGaussRadiusDirect = reader.ReadInt32();
				PVRFilteringGaussRadiusIndirect = reader.ReadInt32();
				PVRFilteringGaussRadiusAO = reader.ReadInt32();
			}
			if (HasPVRFilteringAtrousColorSigma(reader.Version))
			{
				PVRFilteringAtrousColorSigma = reader.ReadSingle();
				PVRFilteringAtrousNormalSigma = reader.ReadSingle();
				PVRFilteringAtrousPositionSigma = reader.ReadSingle();
			}
			else if (HasPVRFilteringAtrousPositionSigmaDirect(reader.Version))
			{
				PVRFilteringAtrousPositionSigmaDirect = reader.ReadSingle();
				PVRFilteringAtrousPositionSigmaIndirect = reader.ReadSingle();
				PVRFilteringAtrousPositionSigmaAO = reader.ReadSingle();
			}
			if (HasShowResolutionOverlay(reader.Version))
			{
				ShowResolutionOverlay = reader.ReadBoolean();
			}
			if (IsAlign2(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasExportTrainingData(reader.Version))
			{
				ExportTrainingData = reader.ReadBoolean();
				reader.AlignStream();
			}
			if (HasTrainingDataDestination(reader.Version))
			{
				TrainingDataDestination = reader.ReadString();
			}
			if (HasLightProbeSampleCountMultiplier(reader.Version))
			{
				LightProbeSampleCountMultiplier = reader.ReadSingle();
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Resolution);
			if (HasBakeResolution(writer.Version))
			{
				writer.Write(BakeResolution);
			}
			if (HasSystemTexelWidth(writer.Version))
			{
				writer.Write(SystemTexelWidth);
			}
			if (HasLastUsedResolution(writer.Version))
			{
				writer.Write(LastUsedResolution);
			}
			if (HasAtlasSize(writer.Version))
			{
				writer.Write(AtlasSize);
			}
			else
			{
				writer.Write(TextureWidth);
				writer.Write(TextureHeight);
			}

			if (HasBounceBoost(writer.Version))
			{
				writer.Write(BounceBoost);
				writer.Write(BounceIntensity);
				SkyLightColor.Write(writer);
				writer.Write(SkyLightIntensity);
				writer.Write(Quality);
				writer.Write(Bounces);
				writer.Write(FinalGatherRays);
				writer.Write(FinalGatherContrastThreshold);
				writer.Write(FinalGatherGradientThreshold);
				writer.Write(FinalGatherInterpolationPoints);
			}

			if (HasAO(writer.Version))
			{
				writer.Write(AO);
				writer.AlignStream();
			}

			if (HasAOAmount(writer.Version))
			{
				writer.Write(AOAmount);
			}

			writer.Write(AOMaxDistance);
			if (HasAOContrast(writer.Version))
			{
				writer.Write(AOContrast);
			}
			if (HasCompAOExponent(writer.Version) && CompAOExponentFirst(writer.Version))
			{
				writer.Write(CompAOExponent);
			}
			if (HasCompAOExponentDirect(writer.Version))
			{
				writer.Write(CompAOExponentDirect);
			}
			if (HasExtractAmbientOcclusion(writer.Version))
			{
				writer.Write(ExtractAmbientOcclusion);
				writer.AlignStream();
			}
			if (HasLODSurfaceMappingDistance(writer.Version))
			{
				writer.Write(LODSurfaceMappingDistance);
			}
			if (HasPadding(writer.Version))
			{
				writer.Write(Padding);
			}
			if (HasCompDirectScale(writer.Version))
			{
				writer.Write(CompDirectScale);
				writer.Write(CompIndirectScale);
			}
			if (HasCompAOExponent(writer.Version) && !CompAOExponentFirst(writer.Version))
			{
				writer.Write(CompAOExponent);
			}
			if (HasLightmapParameters(writer.Version))
			{
				LightmapParameters.Write(writer);
			}
			if (HasLightmapsBakeMode(writer.Version))
			{
				writer.Write((int)LightmapsBakeMode);
			}

			writer.Write(TextureCompression);
			if (HasLockAtlas(writer.Version))
			{
				writer.Write(LockAtlas);
			}
			if (HasDirectLightInLightProbes(writer.Version))
			{
				writer.Write(DirectLightInLightProbes);
			}
			if (HasFinalGather(writer.Version))
			{
				writer.Write(FinalGather);
			}
			if (HasFinalGatherFiltering(writer.Version))
			{
				writer.Write(FinalGatherFiltering);
			}
			if (IsAlign1(writer.Version))
			{
				writer.AlignStream();
			}

			if (HasFinalGatherRayCount(writer.Version))
			{
				writer.Write(FinalGatherRayCount);
			}
			if (HasReflectionCompression(writer.Version))
			{
				writer.Write((int)ReflectionCompression);
			}
			if (HasMixedBakeMode(writer.Version))
			{
				writer.Write((int)MixedBakeMode);
			}
			if (HasBakeBackend(writer.Version))
			{
				writer.Write((int)BakeBackend);
				writer.Write((int)PVRSampling);
			}
			if (HasPVRDirectSampleCount(writer.Version))
			{
				writer.Write(PVRDirectSampleCount);
			}
			if (HasPVRSampleCount(writer.Version))
			{
				writer.Write(PVRSampleCount);
				writer.Write(PVRBounces);
			}
			if (HasPVREnvironmentSampleCount(writer.Version))
			{
				writer.Write(PVREnvironmentSampleCount);
				writer.Write(PVREnvironmentReferencePointCount);
				writer.Write((int)PVRFilteringMode);
				writer.Write((int)PVRDenoiserTypeDirect);
				writer.Write((int)PVRDenoiserTypeIndirect);
				writer.Write((int)PVRDenoiserTypeAO);
			}
			if (HasPVRFiltering(writer.Version))
			{
				writer.Write(PVRFiltering);
			}
			else if (HasPVRFilterTypeDirect(writer.Version))
			{
				writer.Write((int)PVRFilterTypeDirect);
				writer.Write((int)PVRFilterTypeIndirect);
				writer.Write((int)PVRFilterTypeAO);
			}
			if (HasPVREnvironmentMIS(writer.Version))
			{
				writer.Write(PVREnvironmentMIS);
			}
			if (HasPVRFilteringMode(writer.Version) && !HasPVREnvironmentSampleCount(writer.Version))
			{
				writer.Write((int)PVRFilteringMode);
			}
			if (HasPVRCulling(writer.Version))
			{
				writer.Write(PVRCulling);
				writer.AlignStream();

				writer.Write(PVRFilteringGaussRadiusDirect);
				writer.Write(PVRFilteringGaussRadiusIndirect);
				writer.Write(PVRFilteringGaussRadiusAO);
			}
			if (HasPVRFilteringAtrousColorSigma(writer.Version))
			{
				writer.Write(PVRFilteringAtrousColorSigma);
				writer.Write(PVRFilteringAtrousNormalSigma);
				writer.Write(PVRFilteringAtrousPositionSigma);
			}
			else if (HasPVRFilteringAtrousPositionSigmaDirect(writer.Version))
			{
				writer.Write(PVRFilteringAtrousPositionSigmaDirect);
				writer.Write(PVRFilteringAtrousPositionSigmaIndirect);
				writer.Write(PVRFilteringAtrousPositionSigmaAO);
			}
			if (HasShowResolutionOverlay(writer.Version))
			{
				writer.Write(ShowResolutionOverlay);
			}
			if (IsAlign2(writer.Version))
			{
				writer.AlignStream();
			}

			if (HasExportTrainingData(writer.Version))
			{
				writer.Write(ExportTrainingData);
				writer.AlignStream();
			}
			if (HasTrainingDataDestination(writer.Version))
			{
				writer.Write(TrainingDataDestination);
			}
			if (HasLightProbeSampleCountMultiplier(writer.Version))
			{
				writer.Write(LightProbeSampleCountMultiplier);
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(LightmapParameters, LightmapParametersName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(ResolutionName, Resolution);
			if (HasBakeResolution(container.ExportVersion))
			{
				node.Add(BakeResolutionName, BakeResolution);
			}
			if (HasSystemTexelWidth(container.ExportVersion))
			{
				node.Add(SystemTexelWidthName, SystemTexelWidth);
			}
			if (HasLastUsedResolution(container.ExportVersion))
			{
				node.Add(LastUsedResolutionName, LastUsedResolution);
			}
			if (HasAtlasSize(container.ExportVersion))
			{
				node.Add(AtlasSizeName, AtlasSize);
			}
			else
			{
				node.Add(TextureWidthName, TextureWidth);
				node.Add(TextureHeightName, TextureHeight);
			}

			if (HasBounceBoost(container.ExportVersion))
			{
				node.Add(BounceBoostName, BounceBoost);
				node.Add(BounceIntensityName, BounceIntensity);
				node.Add(SkyLightColorName, SkyLightColor.ExportYAML(container));
				node.Add(SkyLightIntensityName, SkyLightIntensity);
				node.Add(QualityName, Quality);
				node.Add(BouncesName, Bounces);
				node.Add(FinalGatherRaysName, FinalGatherRays);
				node.Add(FinalGatherContrastThresholdName, FinalGatherContrastThreshold);
				node.Add(FinalGatherGradientThresholdName, FinalGatherGradientThreshold);
				node.Add(FinalGatherInterpolationPointsName, FinalGatherInterpolationPoints);
			}

			if (HasAO(container.ExportVersion))
			{
				node.Add(AOName, AO);
			}

			if (HasAOAmount(container.ExportVersion))
			{
				node.Add(AOAmountName, AOAmount);
			}

			node.Add(AOMaxDistanceName, AOMaxDistance);
			if (HasAOContrast(container.ExportVersion))
			{
				node.Add(AOContrastName, AOContrast);
			}
			if (HasCompAOExponent(container.ExportVersion) && CompAOExponentFirst(container.ExportVersion))
			{
				node.Add(CompAOExponentName, CompAOExponent);
			}
			if (HasCompAOExponentDirect(container.ExportVersion))
			{
				node.Add(CompAOExponentDirectName, CompAOExponentDirect);
			}
			if (HasExtractAmbientOcclusion(container.ExportVersion))
			{
				node.Add(ExtractAmbientOcclusionName, ExtractAmbientOcclusion);
			}
			if (HasLODSurfaceMappingDistance(container.ExportVersion))
			{
				node.Add(LODSurfaceMappingDistanceName, LODSurfaceMappingDistance);
			}
			if (HasPadding(container.ExportVersion))
			{
				node.Add(PaddingName, Padding);
			}
			if (HasCompDirectScale(container.ExportVersion))
			{
				node.Add(CompDirectScaleName, CompDirectScale);
				node.Add(CompIndirectScaleName, CompIndirectScale);
			}
			if (HasCompAOExponent(container.ExportVersion) && !CompAOExponentFirst(container.ExportVersion))
			{
				node.Add(CompAOExponentName, CompAOExponent);
			}
			if (HasLightmapParameters(container.ExportVersion))
			{
				node.Add(LightmapParametersName, LightmapParameters.ExportYAML(container));
			}
			if (HasLightmapsBakeMode(container.ExportVersion))
			{
				node.Add(LightmapsBakeModeName, (int)LightmapsBakeMode);
			}

			node.Add(TextureCompressionName, TextureCompression);
			if (HasLockAtlas(container.ExportVersion))
			{
				node.Add(LockAtlasName, LockAtlas);
			}
			if (HasDirectLightInLightProbes(container.ExportVersion))
			{
				node.Add(DirectLightInLightProbesName, DirectLightInLightProbes);
			}
			if (HasFinalGather(container.ExportVersion))
			{
				node.Add(FinalGatherName, FinalGather);
			}
			if (HasFinalGatherFiltering(container.ExportVersion))
			{
				node.Add(FinalGatherFilteringName, FinalGatherFiltering);
			}
			if (HasFinalGatherRayCount(container.ExportVersion))
			{
				node.Add(FinalGatherRayCountName, FinalGatherRayCount);
			}
			if (HasReflectionCompression(container.ExportVersion))
			{
				node.Add(ReflectionCompressionName, (int)ReflectionCompression);
			}
			if (HasMixedBakeMode(container.ExportVersion))
			{
				node.Add(MixedBakeModeName, (int)MixedBakeMode);
			}
			if (HasBakeBackend(container.ExportVersion))
			{
				node.Add(BakeBackendName, (int)BakeBackend);
				node.Add(PVRSamplingName, (int)PVRSampling);
			}
			if (HasPVRDirectSampleCount(container.ExportVersion))
			{
				node.Add(PVRDirectSampleCountName, PVRDirectSampleCount);
			}
			if (HasPVRSampleCount(container.ExportVersion))
			{
				node.Add(PVRSampleCountName, PVRSampleCount);
				node.Add(PVRBouncesName, PVRBounces);
			}
			if (HasPVREnvironmentSampleCount(container.ExportVersion))
			{
				node.Add(PVREnvironmentSampleCountName, GetPVREnvironmentSampleCount(container.Version));
				node.Add(PVREnvironmentReferencePointCountName, GetPVREnvironmentReferencePointCount(container.Version));
				node.Add(PVRFilteringModeName, (int)GetPVRFilteringMode(container.Version));
				node.Add(PVRDenoiserTypeDirectName, (int)GetPVRDenoiserTypeDirect(container.Version));
				node.Add(PVRDenoiserTypeIndirectName, (int)GetPVRDenoiserTypeIndirect(container.Version));
				node.Add(PVRDenoiserTypeAOName, (int)GetPVRDenoiserTypeAO(container.Version));
			}
			if (HasPVRFiltering(container.ExportVersion))
			{
				node.Add(PVRFilteringName, PVRFiltering);
			}
			else if (HasPVRFilterTypeDirect(container.ExportVersion))
			{
				node.Add(PVRFilterTypeDirectName, (int)GetPVRFilterTypeDirect(container.Version));
				node.Add(PVRFilterTypeIndirectName, (int)PVRFilterTypeIndirect);
				node.Add(PVRFilterTypeAOName, (int)GetPVRFilterTypeAO(container.Version));
			}
			if (HasPVREnvironmentMIS(container.ExportVersion))
			{
				node.Add(PVREnvironmentMISName, GetPVREnvironmentMIS(container.Version));
			}
			if (HasPVRFilteringMode(container.ExportVersion) && !HasPVREnvironmentSampleCount(container.ExportVersion))
			{
				node.Add(PVRFilteringModeName, (int)PVRFilteringMode);
			}
			if (HasPVRCulling(container.ExportVersion))
			{
				node.Add(PVRCullingName, PVRCulling);
				node.Add(PVRFilteringGaussRadiusDirectName, GetPVRFilteringGaussRadiusDirect(container.Version));
				node.Add(PVRFilteringGaussRadiusIndirectName, GetPVRFilteringGaussRadiusIndirect(container.Version));
				node.Add(PVRFilteringGaussRadiusAOName, GetPVRFilteringGaussRadiusAO(container.Version));
			}
			if (HasPVRFilteringAtrousColorSigma(container.ExportVersion))
			{
				node.Add(PVRFilteringAtrousColorSigmaName, PVRFilteringAtrousColorSigma);
				node.Add(PVRFilteringAtrousNormalSigmaName, PVRFilteringAtrousNormalSigma);
				node.Add(PVRFilteringAtrousPositionSigmaName, PVRFilteringAtrousPositionSigma);
			}
			else if (HasPVRFilteringAtrousPositionSigmaDirect(container.ExportVersion))
			{
				node.Add(PVRFilteringAtrousPositionSigmaDirectName, PVRFilteringAtrousPositionSigmaDirect);
				node.Add(PVRFilteringAtrousPositionSigmaIndirectName, PVRFilteringAtrousPositionSigmaIndirect);
				node.Add(PVRFilteringAtrousPositionSigmaAOName, PVRFilteringAtrousPositionSigmaAO);
			}
			if (HasShowResolutionOverlay(container.ExportVersion))
			{
				node.Add(ShowResolutionOverlayName, ShowResolutionOverlay);
			}
			if (HasExportTrainingData(container.ExportVersion))
			{
				node.Add(ExportTrainingDataName, ExportTrainingData);
			}
			if (HasTrainingDataDestination(container.ExportVersion))
			{
				node.Add(TrainingDataDestinationName, GetTrainingDataDestination(container.Version));
			}
			if (HasLightProbeSampleCountMultiplier(container.ExportVersion))
			{
				node.Add(LightProbeSampleCountMultiplierName, GetLightProbeSampleCountMultiplier(container.Version));
			}
			return node;
		}

		private int GetPVREnvironmentSampleCount(Version version)
		{
			return HasPVREnvironmentSampleCount(version) ? PVREnvironmentSampleCount : PVRSampleCount;
		}
		private int GetPVREnvironmentReferencePointCount(Version version)
		{
			return HasPVREnvironmentSampleCount(version) ? PVREnvironmentReferencePointCount : 2048;
		}
		private FilterMode GetPVRFilteringMode(Version version)
		{
			if (ToSerializedVersion(version) < 11)
			{
				if (PVRFilteringMode == FilterMode.Auto)
				{
					return FilterMode.Advanced;
				}
			}
			return PVRFilteringMode;
		}
		private DenoiserType GetPVRDenoiserTypeDirect(Version version)
		{
			return HasPVREnvironmentSampleCount(version) ? PVRDenoiserTypeDirect : DenoiserType.None;
		}
		private DenoiserType GetPVRDenoiserTypeIndirect(Version version)
		{
			return HasPVREnvironmentSampleCount(version) ? PVRDenoiserTypeIndirect : DenoiserType.Optix;
		}
		private DenoiserType GetPVRDenoiserTypeAO(Version version)
		{
			return HasPVREnvironmentSampleCount(version) ? PVRDenoiserTypeAO : DenoiserType.None;
		}
		private FilterType GetPVRFilterTypeDirect(Version version)
		{
			if (ToSerializedVersion(version) < 11)
			{
				if (PVRFilteringMode == FilterMode.Auto)
				{
					return FilterType.Gaussian;
				}
			}
			return PVRFilterTypeDirect;
		}
		private FilterType GetPVRFilterTypeAO(Version version)
		{
			if (ToSerializedVersion(version) < 11)
			{
				if (PVRFilteringMode == FilterMode.Auto)
				{
					return FilterType.Gaussian;
				}
			}
			return PVRFilterTypeAO;
		}
		private int GetPVREnvironmentMIS(Version version)
		{
			return HasPVREnvironmentMIS(version) ? PVREnvironmentMIS : 0;
		}
		private int GetPVRFilteringGaussRadiusDirect(Version version)
		{
			if (ToSerializedVersion(version) < 11)
			{
				if (PVRFilteringMode == FilterMode.Auto)
				{
					return 1;
				}
			}
			return PVRFilteringGaussRadiusDirect;
		}
		private int GetPVRFilteringGaussRadiusIndirect(Version version)
		{
			if (ToSerializedVersion(version) < 11)
			{
				if (PVRFilteringMode == FilterMode.Auto)
				{
					return 5;
				}
			}
			return PVRFilteringGaussRadiusIndirect;
		}
		private int GetPVRFilteringGaussRadiusAO(Version version)
		{
			if (ToSerializedVersion(version) < 11)
			{
				if (PVRFilteringMode == FilterMode.Auto)
				{
					return 2;
				}
			}
			return PVRFilteringGaussRadiusAO;
		}
		private string GetTrainingDataDestination(Version version)
		{
			return HasTrainingDataDestination(version) ? TrainingDataDestination : TrainingDataName;
		}
		private float GetLightProbeSampleCountMultiplier(Version version)
		{
			return HasLightProbeSampleCountMultiplier(version) ? LightProbeSampleCountMultiplier : 4.0f;
		}

		public float Resolution { get; set; }
		public float BakeResolution { get; set; }
		public int SystemTexelWidth { get; set; }
		public float LastUsedResolution { get; set; }
		public int AtlasSize
		{
			get => TextureWidth;
			set => TextureWidth = TextureHeight = value;
		}
		public int TextureWidth { get; set; }
		public int TextureHeight { get; set; }
		public float BounceBoost { get; set; }
		public float BounceIntensity { get; set; }
		public float SkyLightIntensity { get; set; }
		public int Quality { get; set; }
		public int Bounces { get; set; }
		public int FinalGatherRays { get; set; }
		public float FinalGatherContrastThreshold { get; set; }
		public float FinalGatherGradientThreshold { get; set; }
		public int FinalGatherInterpolationPoints { get; set; }
		public bool AO { get; set; }
		public float AOAmount { get; set; }
		public float AOMaxDistance { get; set; }
		public float AOContrast { get; set; }
		public float CompDirectScale { get; set; }
		public float CompIndirectScale { get; set; }
		public float CompAOExponent { get; set; }
		public float CompAOExponentDirect { get; set; }
		public bool ExtractAmbientOcclusion { get; set; }
		public float LODSurfaceMappingDistance { get; set; }
		public int Padding { get; set; }
		public PPtr<LightmapParameters> LightmapParameters { get; set; }
		public LightmapsMode LightmapsBakeMode { get; set; }
		public bool TextureCompression { get; set; }
		public bool LockAtlas { get; set; }
		public bool DirectLightInLightProbes { get; set; }
		public bool FinalGather { get; set; }
		public bool FinalGatherFiltering { get; set; }
		public int FinalGatherRayCount { get; set; }
		public ReflectionCubemapCompression ReflectionCompression { get; set; }
		/// <summary>
		/// StationaryBakeMode previously (before 5.6.0b8)
		/// </summary>
		public MixedLightingMode MixedBakeMode { get; set; }
		public Lightmapper BakeBackend { get; set; }
		public Sampling PVRSampling { get; set; }
		public int PVRDirectSampleCount { get; set; }
		public int PVRSampleCount { get; set; }
		public int PVRBounces { get; set; }
		public int PVREnvironmentSampleCount { get; set; }
		public int PVREnvironmentReferencePointCount { get; set; }
		public FilterMode PVRFilteringMode { get; set; }
		public DenoiserType PVRDenoiserTypeDirect { get; set; }
		public DenoiserType PVRDenoiserTypeIndirect { get; set; }
		public DenoiserType PVRDenoiserTypeAO { get; set; }
		public FilterType PVRFilterTypeDirect { get; set; }
		public FilterType PVRFilterTypeIndirect { get; set; }
		public FilterType PVRFilterTypeAO { get; set; }
		public int PVREnvironmentMIS { get; set; }
		public int PVRFiltering { get; set; }
		public bool PVRCulling { get; set; }
		public int PVRFilteringGaussRadiusDirect { get; set; }
		public int PVRFilteringGaussRadiusIndirect { get; set; }
		public int PVRFilteringGaussRadiusAO { get; set; }
		public float PVRFilteringAtrousColorSigma { get; set; }
		public float PVRFilteringAtrousNormalSigma { get; set; }
		public float PVRFilteringAtrousPositionSigma { get; set; }		
		public float PVRFilteringAtrousPositionSigmaDirect { get; set; }
		public float PVRFilteringAtrousPositionSigmaIndirect { get; set; }
		public float PVRFilteringAtrousPositionSigmaAO { get; set; }
		public bool ShowResolutionOverlay { get; set; }
		public bool ExportTrainingData { get; set; }
		public string TrainingDataDestination { get; set; }
		public float LightProbeSampleCountMultiplier { get; set; }

		public const string ResolutionName = "m_Resolution";
		public const string BakeResolutionName = "m_BakeResolution";
		public const string SystemTexelWidthName = "m_SystemTexelWidth";
		public const string LastUsedResolutionName = "m_LastUsedResolution";
		public const string TextureWidthName = "m_TextureWidth";
		public const string TextureHeightName = "m_TextureHeight";
		public const string AtlasSizeName = "m_AtlasSize";
		public const string BounceBoostName = "m_BounceBoost";
		public const string BounceIntensityName = "m_BounceIntensity";
		public const string SkyLightColorName = "m_SkyLightColor";
		public const string SkyLightIntensityName = "m_SkyLightIntensity";
		public const string QualityName = "m_Quality";
		public const string BouncesName = "m_Bounces";
		public const string FinalGatherRaysName = "m_FinalGatherRays";
		public const string FinalGatherContrastThresholdName = "m_FinalGatherContrastThreshold";
		public const string FinalGatherGradientThresholdName = "m_FinalGatherGradientThreshold";
		public const string FinalGatherInterpolationPointsName = "m_FinalGatherInterpolationPoints";
		public const string AOName = "m_AO";
		public const string AOAmountName = "m_AOAmount";
		public const string AOMaxDistanceName = "m_AOMaxDistance";
		public const string AOContrastName = "m_AOContrast";
		public const string CompAOExponentDirectName = "m_CompAOExponentDirect";
		public const string ExtractAmbientOcclusionName = "m_ExtractAmbientOcclusion";
		public const string LODSurfaceMappingDistanceName = "m_LODSurfaceMappingDistance";
		public const string PaddingName = "m_Padding";
		public const string CompDirectScaleName = "m_CompDirectScale";
		public const string CompIndirectScaleName = "m_CompIndirectScale";
		public const string CompAOExponentName = "m_CompAOExponent";
		public const string LightmapParametersName = "m_LightmapParameters";
		public const string LightmapsBakeModeName = "m_LightmapsBakeMode";
		public const string TextureCompressionName = "m_TextureCompression";
		public const string LockAtlasName = "m_LockAtlas";
		public const string DirectLightInLightProbesName = "m_DirectLightInLightProbes";
		public const string FinalGatherName = "m_FinalGather";
		public const string FinalGatherFilteringName = "m_FinalGatherFiltering";
		public const string FinalGatherRayCountName = "m_FinalGatherRayCount";
		public const string ReflectionCompressionName = "m_ReflectionCompression";
		public const string StationaryBakeModeName = "m_StationaryBakeMode";
		public const string MixedBakeModeName = "m_MixedBakeMode";
		public const string BakeBackendName = "m_BakeBackend";
		public const string PVRSamplingName = "m_PVRSampling";
		public const string PVRDirectSampleCountName = "m_PVRDirectSampleCount";
		public const string PVRSampleCountName = "m_PVRSampleCount";
		public const string PVRBouncesName = "m_PVRBounces";
		public const string PVREnvironmentSampleCountName = "m_PVREnvironmentSampleCount";
		public const string PVREnvironmentReferencePointCountName = "m_PVREnvironmentReferencePointCount";
		public const string PVRFilteringModeName = "m_PVRFilteringMode";
		public const string PVRDenoiserTypeDirectName = "m_PVRDenoiserTypeDirect";
		public const string PVRDenoiserTypeIndirectName = "m_PVRDenoiserTypeIndirect";
		public const string PVRDenoiserTypeAOName = "m_PVRDenoiserTypeAO";
		public const string PVRFilteringName = "m_PVRFiltering";
		public const string PVRFilterTypeDirectName = "m_PVRFilterTypeDirect";
		public const string PVRFilterTypeIndirectName = "m_PVRFilterTypeIndirect";
		public const string PVRFilterTypeAOName = "m_PVRFilterTypeAO";
		public const string PVREnvironmentMISName = "m_PVREnvironmentMIS";
		public const string PVRCullingName = "m_PVRCulling";
		public const string PVRFilteringGaussRadiusDirectName = "m_PVRFilteringGaussRadiusDirect";
		public const string PVRFilteringGaussRadiusIndirectName = "m_PVRFilteringGaussRadiusIndirect";
		public const string PVRFilteringGaussRadiusAOName = "m_PVRFilteringGaussRadiusAO";
		public const string PVRFilteringAtrousColorSigmaName = "m_PVRFilteringAtrousColorSigma";
		public const string PVRFilteringAtrousNormalSigmaName = "m_PVRFilteringAtrousNormalSigma";
		public const string PVRFilteringAtrousPositionSigmaName = "m_PVRFilteringAtrousPositionSigma";
		public const string PVRFilteringAtrousPositionSigmaDirectName = "m_PVRFilteringAtrousPositionSigmaDirect";
		public const string PVRFilteringAtrousPositionSigmaIndirectName = "m_PVRFilteringAtrousPositionSigmaIndirect";
		public const string PVRFilteringAtrousPositionSigmaAOName = "m_PVRFilteringAtrousPositionSigmaAO";
		public const string ShowResolutionOverlayName = "m_ShowResolutionOverlay";
		public const string ExportTrainingDataName = "m_ExportTrainingData";
		public const string TrainingDataDestinationName = "m_TrainingDataDestination";
		public const string LightProbeSampleCountMultiplierName = "m_LightProbeSampleCountMultiplier";

		private const string TrainingDataName = "TrainingData";

		public ColorRGBAf SkyLightColor;
	}
}
