using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Lights;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct LightmapEditorSettings
	{
		public LightmapEditorSettings(bool _)
		{
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
		}

		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadAtlasSize(Version version)
		{
			return version.IsGreaterEqual(2018);
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadExtractAmbientOcclusion(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadPVREnvironmentSampleCount(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// Less than 2019.2
		/// </summary>
		public static bool IsReadShowResolutionOverlay(Version version)
		{
			return version.IsLess(2019, 2);
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadPVREnvironmentMIS(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadExportTrainingData(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 2019.2 and greater
		/// </summary>
		public static bool IsReadTrainingDataDestination(Version version)
		{
			return version.IsGreaterEqual(2019, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			// PVREnvironmentMIS default value has been changed from 0 to 1
			// PVREnvironmentSampleCount not equals to PVRSampleCount anymore
			if (version.IsGreaterEqual(2019))
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
			// unknown version
			// return 11;

			// TextureWidth and TextureHeight has been replaced by AtlasSize
			if (version.IsGreaterEqual(2018))
			{
				return 10;
			}

			// TODO:
			return 9;

			/*if (version.IsGreaterEqual())
			{
				return 9;
			}
			if (version.IsGreaterEqual())
			{
				return 8;
			}
			if (version.IsGreaterEqual())
			{
				return 7;
			}
			if (version.IsGreaterEqual())
			{
				return 6;
			}
			if (version.IsGreaterEqual())
			{
				return 5;
			}
			if (version.IsGreaterEqual())
			{
				return 4;
			}
			if (version.IsGreaterEqual())
			{
				return 3;
			}
			if (version.IsGreaterEqual())
			{
				return 2;
			}
			return 1;*/
		}


		public void Read(AssetReader reader)
		{
			Resolution = reader.ReadSingle();
			BakeResolution = reader.ReadSingle();
			if (IsReadAtlasSize(reader.Version))
			{
				int AtlasSize = reader.ReadInt32();
				TextureWidth = TextureHeight = AtlasSize;
			}
			else
			{
				TextureWidth = reader.ReadInt32();
				TextureHeight = reader.ReadInt32();
			}
			AO = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);

			AOMaxDistance = reader.ReadSingle();
			CompAOExponent = reader.ReadSingle();
			CompAOExponentDirect = reader.ReadSingle();
			if (IsReadExtractAmbientOcclusion(reader.Version))
			{
				ExtractAmbientOcclusion = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			Padding = reader.ReadInt32();
			LightmapParameters.Read(reader);
			LightmapsBakeMode = (LightmapsMode)reader.ReadInt32();
			TextureCompression = reader.ReadBoolean();
			FinalGather = reader.ReadBoolean();
			FinalGatherFiltering = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);

			FinalGatherRayCount = reader.ReadInt32();
			ReflectionCompression = (ReflectionCubemapCompression)reader.ReadInt32();
			MixedBakeMode = (MixedLightingMode)reader.ReadInt32();
			BakeBackend = (Lightmapper)reader.ReadInt32();
			PVRSampling = (Sampling)reader.ReadInt32();
			PVRDirectSampleCount = reader.ReadInt32();
			PVRSampleCount = reader.ReadInt32();
			PVRBounces = reader.ReadInt32();
			if (IsReadPVREnvironmentSampleCount(reader.Version))
			{
				PVREnvironmentSampleCount = reader.ReadInt32();
				PVREnvironmentReferencePointCount = reader.ReadInt32();
				PVRFilteringMode = (FilterMode)reader.ReadInt32();
				PVRDenoiserTypeDirect = (DenoiserType)reader.ReadInt32();
				PVRDenoiserTypeIndirect = (DenoiserType)reader.ReadInt32();
				PVRDenoiserTypeAO = (DenoiserType)reader.ReadInt32();
			}
			PVRFilterTypeDirect = (FilterType)reader.ReadInt32();
			PVRFilterTypeIndirect = (FilterType)reader.ReadInt32();
			PVRFilterTypeAO = (FilterType)reader.ReadInt32();
			if (IsReadPVREnvironmentMIS(reader.Version))
			{
				PVREnvironmentMIS = reader.ReadInt32();
			}
			if (!IsReadPVREnvironmentSampleCount(reader.Version))
			{
				PVRFilteringMode = (FilterMode)reader.ReadInt32();
			}
			PVRCulling = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);

			PVRFilteringGaussRadiusDirect = reader.ReadInt32();
			PVRFilteringGaussRadiusIndirect = reader.ReadInt32();
			PVRFilteringGaussRadiusAO = reader.ReadInt32();
			PVRFilteringAtrousPositionSigmaDirect = reader.ReadSingle();
			PVRFilteringAtrousPositionSigmaIndirect = reader.ReadSingle();
			PVRFilteringAtrousPositionSigmaAO = reader.ReadSingle();
			if (IsReadShowResolutionOverlay(reader.Version))
			{
				ShowResolutionOverlay = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);

			if (IsReadExportTrainingData(reader.Version))
			{
				ExportTrainingData = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadTrainingDataDestination(reader.Version))
			{
				TrainingDataDestination = reader.ReadString();
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return LightmapParameters.FetchDependency(file, isLog, () => nameof(LightmapEditorSettings), LightmapParametersName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(ResolutionName, Resolution);
			node.Add(BakeResolutionName, BakeResolution);
			if (IsReadAtlasSize(container.Version))
			{
				node.Add(AtlasSizeName, AtlasSize);
			}
			else
			{
				node.Add(TextureWidthName, TextureWidth);
				node.Add(TextureHeightName, TextureHeight);
			}
			node.Add(AOName, AO);
			node.Add(AOMaxDistanceName, AOMaxDistance);
			node.Add(CompAOExponentName, CompAOExponent);
			node.Add(CompAOExponentDirectName, CompAOExponentDirect);
			if (IsReadExtractAmbientOcclusion(container.ExportVersion))
			{
				node.Add(ExtractAmbientOcclusionName, ExtractAmbientOcclusion);
			}
			node.Add(PaddingName, Padding);
			node.Add(LightmapParametersName, LightmapParameters.ExportYAML(container));
			node.Add(LightmapsBakeModeName, (int)LightmapsBakeMode);
			node.Add(TextureCompressionName, TextureCompression);
			node.Add(FinalGatherName, FinalGather);
			node.Add(FinalGatherFilteringName, FinalGatherFiltering);
			node.Add(FinalGatherRayCountName, FinalGatherRayCount);
			node.Add(ReflectionCompressionName, (int)ReflectionCompression);
			node.Add(MixedBakeModeName, (int)MixedBakeMode);
			node.Add(BakeBackendName, (int)BakeBackend);
			node.Add(PVRSamplingName, (int)PVRSampling);
			node.Add(PVRDirectSampleCountName, PVRDirectSampleCount);
			node.Add(PVRSampleCountName, PVRSampleCount);
			node.Add(PVRBouncesName, PVRBounces);
			if (IsReadPVREnvironmentSampleCount(container.ExportVersion))
			{
				node.Add(PVREnvironmentSampleCountName, GetPVREnvironmentSampleCount(container.Version));
				node.Add(PVREnvironmentReferencePointCountName, GetPVREnvironmentReferencePointCount(container.Version));
			}
			node.Add(PVRFilteringModeName, (int)GetPVRFilteringMode(container.Version));
			if (IsReadPVREnvironmentSampleCount(container.ExportVersion))
			{
				node.Add(PVRDenoiserTypeDirectName, (int)GetPVRDenoiserTypeDirect(container.Version));
				node.Add(PVRDenoiserTypeIndirectName, (int)GetPVRDenoiserTypeIndirect(container.Version));
				node.Add(PVRDenoiserTypeAOName, (int)GetPVRDenoiserTypeAO(container.Version));
			}
			node.Add(PVRFilterTypeDirectName, (int)GetPVRFilterTypeDirect(container.Version));
			node.Add(PVRFilterTypeIndirectName, (int)PVRFilterTypeIndirect);
			node.Add(PVRFilterTypeAOName, (int)GetPVRFilterTypeAO(container.Version));
			if (IsReadPVREnvironmentMIS(container.ExportVersion))
			{
				node.Add(PVREnvironmentMISName, GetPVREnvironmentMIS(container.Version));
			}
			node.Add(PVRCullingName, PVRCulling);
			node.Add(PVRFilteringGaussRadiusDirectName, GetPVRFilteringGaussRadiusDirect(container.Version));
			node.Add(PVRFilteringGaussRadiusIndirectName, GetPVRFilteringGaussRadiusIndirect(container.Version));
			node.Add(PVRFilteringGaussRadiusAOName, GetPVRFilteringGaussRadiusAO(container.Version));
			node.Add(PVRFilteringAtrousPositionSigmaDirectName, PVRFilteringAtrousPositionSigmaDirect);
			node.Add(PVRFilteringAtrousPositionSigmaIndirectName, PVRFilteringAtrousPositionSigmaIndirect);
			node.Add(PVRFilteringAtrousPositionSigmaAOName, PVRFilteringAtrousPositionSigmaAO);
			if (IsReadShowResolutionOverlay(container.ExportVersion))
			{
				node.Add(ShowResolutionOverlayName, ShowResolutionOverlay);
			}
			if (IsReadExportTrainingData(container.ExportVersion))
			{
				node.Add(ExportTrainingDataName, ExportTrainingData);
			}
			if (IsReadTrainingDataDestination(container.ExportVersion))
			{
				node.Add(TrainingDataDestinationName, GetTrainingDataDestination(container.Version));
			}
			return node;
		}

		private int GetPVREnvironmentSampleCount(Version version)
		{
			return IsReadPVREnvironmentSampleCount(version) ? PVREnvironmentSampleCount : PVRSampleCount;
		}
		private int GetPVREnvironmentReferencePointCount(Version version)
		{
			return IsReadPVREnvironmentSampleCount(version) ? PVREnvironmentReferencePointCount : 2048;
		}
		private FilterMode GetPVRFilteringMode(Version version)
		{
			if (GetSerializedVersion(version) < 11)
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
			return IsReadPVREnvironmentSampleCount(version) ? PVRDenoiserTypeDirect : DenoiserType.None;
		}
		private DenoiserType GetPVRDenoiserTypeIndirect(Version version)
		{
			return IsReadPVREnvironmentSampleCount(version) ? PVRDenoiserTypeIndirect : DenoiserType.Optix;
		}
		private DenoiserType GetPVRDenoiserTypeAO(Version version)
		{
			return IsReadPVREnvironmentSampleCount(version) ? PVRDenoiserTypeAO : DenoiserType.None;
		}
		private FilterType GetPVRFilterTypeDirect(Version version)
		{
			if (GetSerializedVersion(version) < 11)
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
			if (GetSerializedVersion(version) < 11)
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
			return IsReadPVREnvironmentMIS(version) ? PVREnvironmentMIS : 0;
		}
		private int GetPVRFilteringGaussRadiusDirect(Version version)
		{
			if (GetSerializedVersion(version) < 11)
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
			if (GetSerializedVersion(version) < 11)
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
			if (GetSerializedVersion(version) < 11)
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
			return IsReadTrainingDataDestination(version) ? TrainingDataDestination : TrainingDataName;
		}

		public float Resolution { get; private set; }
		public float BakeResolution { get; private set; }
		public int AtlasSize => TextureWidth;
		public int TextureWidth { get; private set; }
		public int TextureHeight { get; private set; }
		public bool AO { get; private set; }
		public float AOMaxDistance { get; private set; }
		public float CompAOExponent { get; private set; }
		public float CompAOExponentDirect { get; private set; }
		public bool ExtractAmbientOcclusion { get; private set; }
		public int Padding { get; private set; }
		public PPtr<LightmapParameters> LightmapParameters { get; private set; }
		public LightmapsMode LightmapsBakeMode { get; private set; }
		public bool TextureCompression { get; private set; }
		public bool FinalGather { get; private set; }
		public bool FinalGatherFiltering { get; private set; }
		public int FinalGatherRayCount { get; private set; }
		public ReflectionCubemapCompression ReflectionCompression { get; private set; }
		public MixedLightingMode MixedBakeMode { get; private set; }
		public Lightmapper BakeBackend { get; private set; }
		public Sampling PVRSampling { get; private set; }
		public int PVRDirectSampleCount { get; private set; }
		public int PVRSampleCount { get; private set; }
		public int PVRBounces { get; private set; }
		public int PVREnvironmentSampleCount { get; private set; }
		public int PVREnvironmentReferencePointCount { get; private set; }
		public FilterMode PVRFilteringMode { get; private set; }
		public DenoiserType PVRDenoiserTypeDirect { get; private set; }
		public DenoiserType PVRDenoiserTypeIndirect { get; private set; }
		public DenoiserType PVRDenoiserTypeAO { get; private set; }
		public FilterType PVRFilterTypeDirect { get; private set; }
		public FilterType PVRFilterTypeIndirect { get; private set; }
		public FilterType PVRFilterTypeAO { get; private set; }
		public int PVREnvironmentMIS { get; private set; }
		public bool PVRCulling { get; private set; }
		public int PVRFilteringGaussRadiusDirect { get; private set; }
		public int PVRFilteringGaussRadiusIndirect { get; private set; }
		public int PVRFilteringGaussRadiusAO { get; private set; }
		public float PVRFilteringAtrousPositionSigmaDirect { get; private set; }
		public float PVRFilteringAtrousPositionSigmaIndirect { get; private set; }
		public float PVRFilteringAtrousPositionSigmaAO { get; private set; }
		public bool ShowResolutionOverlay { get; private set; }
		public bool ExportTrainingData { get; private set; }
		public string TrainingDataDestination { get; private set; }

		public const string ResolutionName = "m_Resolution";
		public const string BakeResolutionName = "m_BakeResolution";
		public const string TextureWidthName = "m_TextureWidth";
		public const string TextureHeightName = "m_TextureHeight";
		public const string AtlasSizeName = "m_AtlasSize";
		public const string AOName = "m_AO";
		public const string AOMaxDistanceName = "m_AOMaxDistance";
		public const string CompAOExponentName = "m_CompAOExponent";
		public const string CompAOExponentDirectName = "m_CompAOExponentDirect";
		public const string ExtractAmbientOcclusionName = "m_ExtractAmbientOcclusion";
		public const string PaddingName = "m_Padding";
		public const string LightmapParametersName = "m_LightmapParameters";
		public const string LightmapsBakeModeName = "m_LightmapsBakeMode";
		public const string TextureCompressionName = "m_TextureCompression";
		public const string FinalGatherName = "m_FinalGather";
		public const string FinalGatherFilteringName = "m_FinalGatherFiltering";
		public const string FinalGatherRayCountName = "m_FinalGatherRayCount";
		public const string ReflectionCompressionName = "m_ReflectionCompression";
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
		public const string PVRFilterTypeDirectName = "m_PVRFilterTypeDirect";
		public const string PVRFilterTypeIndirectName = "m_PVRFilterTypeIndirect";
		public const string PVRFilterTypeAOName = "m_PVRFilterTypeAO";
		public const string PVREnvironmentMISName = "m_PVREnvironmentMIS";
		public const string PVRCullingName = "m_PVRCulling";
		public const string PVRFilteringGaussRadiusDirectName = "m_PVRFilteringGaussRadiusDirect";
		public const string PVRFilteringGaussRadiusIndirectName = "m_PVRFilteringGaussRadiusIndirect";
		public const string PVRFilteringGaussRadiusAOName = "m_PVRFilteringGaussRadiusAO";
		public const string PVRFilteringAtrousPositionSigmaDirectName = "m_PVRFilteringAtrousPositionSigmaDirect";
		public const string PVRFilteringAtrousPositionSigmaIndirectName = "m_PVRFilteringAtrousPositionSigmaIndirect";
		public const string PVRFilteringAtrousPositionSigmaAOName = "m_PVRFilteringAtrousPositionSigmaAO";
		public const string ShowResolutionOverlayName = "m_ShowResolutionOverlay";
		public const string ExportTrainingDataName = "m_ExportTrainingData";
		public const string TrainingDataDestinationName = "m_TrainingDataDestination";

		private const string TrainingDataName = "TrainingData";
	}
}
