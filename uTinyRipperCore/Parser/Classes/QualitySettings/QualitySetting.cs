using System;
using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.QualitySettingss
{
	public class QualitySetting : IAssetReadable, IYAMLExportable
	{
		public QualitySetting()
		{
		}

		public QualitySetting(bool _)
		{
			ShadowProjection = ShadowProjection.StableFit;
			ShadowNearPlaneOffset = 3.0f;
			ShadowCascade2Split = 1.0f / 3.0f;
			ShadowCascade4Split = new Vector3f(2.0f / 30.0f, 0.2f, 14.0f / 30.0f);
			StreamingMipmapsAddAllCameras = true;
			StreamingMipmapsMemoryBudget = 512.0f;
			StreamingMipmapsRenderersPerFrame = 512;
			StreamingMipmapsMaxLevelReduction = 2;
			StreamingMipmapsMaxFileIORequests = 1024;
			AsyncUploadTimeSlice = 2;
			AsyncUploadBufferSize = 4;
			ResolutionScalingFixedDPIFactor = 1.0f;
#if UNIVERSAL
			ExcludedTargetPlatforms = Array.Empty<string>();
#endif
		}

		public static int ToSerializedVersion(Version version)
		{
			// SyncToVBL has been removed
			if (version.IsGreaterEqual(3, 4))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasName(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasShadows(Version version) => version.IsGreaterEqual(2);
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool HasShadowProjection(Version version) => version.IsGreaterEqual(3, 4);
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasShadowCascades(Version version) => version.IsGreaterEqual(2);
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasShadowNearPlaneOffset(Version version) => version.IsGreaterEqual(5, 2);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasShadowCascade2Split(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasShadowmaskMode(Version version) => version.IsGreaterEqual(2017, 1);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasSoftParticles(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasSoftVegetation(Version version) => version.IsGreaterEqual(2);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasRealtimeReflectionProbes(Version version) => version.IsGreaterEqual(2);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasBillboardsFaceCameraPosition(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		/// <summary>
		/// Less than 3.4.0
		/// </summary>
		public static bool HasSyncToVBL(Version version) => version.IsLess(3, 4);
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool HasVSyncCount(Version version) => version.IsGreaterEqual(3, 4);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasLodBias(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasStreamingMipmapsActive(Version version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasParticleRaycastBudget(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasAsyncUploadTimeSlice(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasAsyncUploadPersistentBuffer(Version version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasResolutionScalingFixedDPIFactor(Version version) => version.IsGreaterEqual(2017, 1);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasCustomRenderPipeline(Version version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool HasExcludedTargetPlatforms(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(3, 5);

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private bool IsAlign(Version version) => version.IsGreaterEqual(2, 1);

		public void Merge(QualitySetting setting, Version version, TransferInstructionFlags flags)
		{
			if (!HasShadows(version))
			{
				Shadows = setting.Shadows;
				ShadowResolution = setting.ShadowResolution;
			}
			if (!HasShadowProjection(version))
			{
				ShadowProjection = setting.ShadowProjection;
			}
			if (!HasShadowCascades(version))
			{
				ShadowCascades = setting.ShadowCascades;
				ShadowDistance = setting.ShadowDistance;
			}
			if (!HasShadowNearPlaneOffset(version))
			{
				ShadowNearPlaneOffset = setting.ShadowNearPlaneOffset;
			}
			if (!HasShadowCascade2Split(version))
			{
				ShadowCascade2Split = setting.ShadowCascade2Split;
				ShadowCascade4Split = setting.ShadowCascade4Split;
			}
			if (!HasShadowmaskMode(version))
			{
				ShadowmaskMode = setting.ShadowmaskMode;
			}
			if (!HasSoftParticles(version))
			{
				SoftParticles = setting.SoftParticles;
			}
			if (!HasSoftVegetation(version))
			{
				SoftVegetation = setting.SoftVegetation;
			}
			if (!HasRealtimeReflectionProbes(version))
			{
				RealtimeReflectionProbes = setting.RealtimeReflectionProbes;
			}
			if (!HasBillboardsFaceCameraPosition(version))
			{
				BillboardsFaceCameraPosition = setting.BillboardsFaceCameraPosition;
			}
			if (!HasSyncToVBL(version))
			{
				SyncToVBL = setting.SyncToVBL;
			}
			if (!HasVSyncCount(version))
			{
				VSyncCount = setting.VSyncCount;
			}
			if (!HasLodBias(version))
			{
				LodBias = setting.LodBias;
				MaximumLODLevel = setting.MaximumLODLevel;
			}
			if (!HasStreamingMipmapsActive(version))
			{
				StreamingMipmapsActive = setting.StreamingMipmapsActive;
				StreamingMipmapsAddAllCameras = setting.StreamingMipmapsAddAllCameras;
				StreamingMipmapsMemoryBudget = setting.StreamingMipmapsMemoryBudget;
				StreamingMipmapsRenderersPerFrame = setting.StreamingMipmapsRenderersPerFrame;
				StreamingMipmapsMaxLevelReduction = setting.StreamingMipmapsMaxLevelReduction;
				StreamingMipmapsMaxFileIORequests = setting.StreamingMipmapsMaxFileIORequests;
			}
			if (!HasParticleRaycastBudget(version))
			{
				ParticleRaycastBudget = setting.ParticleRaycastBudget;
			}
			if (!HasAsyncUploadTimeSlice(version))
			{
				AsyncUploadTimeSlice = setting.AsyncUploadTimeSlice;
				AsyncUploadBufferSize = setting.AsyncUploadBufferSize;
			}
			if (!HasResolutionScalingFixedDPIFactor(version))
			{
				ResolutionScalingFixedDPIFactor = setting.ResolutionScalingFixedDPIFactor;
			}
			if (!HasCustomRenderPipeline(version))
			{
				CustomRenderPipeline = setting.CustomRenderPipeline;
			}
#if UNIVERSAL
			if (!HasExcludedTargetPlatforms(version, flags))
			{
				ExcludedTargetPlatforms = setting.ExcludedTargetPlatforms;
			}
#endif
		}

		public void Read(AssetReader reader)
		{
			if (HasName(reader.Version))
			{
				Name = reader.ReadString();
			}
			PixelLightCount = reader.ReadInt32();
			if (HasShadows(reader.Version))
			{
				Shadows = (ShadowQuality)reader.ReadInt32();
				ShadowResolution = (ShadowResolution)reader.ReadInt32();
			}
			if (HasShadowProjection(reader.Version))
			{
				ShadowProjection = (ShadowProjection)reader.ReadInt32();
			}
			if (HasShadowCascades(reader.Version))
			{
				ShadowCascades = (ShadowCascades)reader.ReadInt32();
				ShadowDistance = reader.ReadSingle();
			}
			if (HasShadowNearPlaneOffset(reader.Version))
			{
				ShadowNearPlaneOffset = reader.ReadSingle();
			}
			if (HasShadowCascade2Split(reader.Version))
			{
				ShadowCascade2Split = reader.ReadSingle();
				ShadowCascade4Split.Read(reader);
			}
			if (HasShadowmaskMode(reader.Version))
			{
				ShadowmaskMode = (ShadowmaskMode)reader.ReadInt32();
			}

			SkinWeights = (SkinWeights)reader.ReadInt32();
			TextureQuality = (TextureQuality)reader.ReadInt32();
			AnisotropicTextures = (AnisotropicFiltering)reader.ReadInt32();
			AntiAliasing = (AntiAliasing)reader.ReadInt32();
			if (HasSoftParticles(reader.Version))
			{
				SoftParticles = reader.ReadBoolean();
			}
			if (HasSoftVegetation(reader.Version))
			{
				SoftVegetation = reader.ReadBoolean();
			}
			if (HasRealtimeReflectionProbes(reader.Version))
			{
				RealtimeReflectionProbes = reader.ReadBoolean();
			}
			if (HasBillboardsFaceCameraPosition(reader.Version))
			{
				BillboardsFaceCameraPosition = reader.ReadBoolean();
			}
			if (HasSyncToVBL(reader.Version))
			{
				SyncToVBL = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}
			if (HasVSyncCount(reader.Version))
			{
				VSyncCount = (VSyncCount)reader.ReadInt32();
			}
			if (HasLodBias(reader.Version))
			{
				LodBias = reader.ReadSingle();
				MaximumLODLevel = reader.ReadInt32();
			}

			if (HasStreamingMipmapsActive(reader.Version))
			{
				StreamingMipmapsActive = reader.ReadBoolean();
				StreamingMipmapsAddAllCameras = reader.ReadBoolean();
				reader.AlignStream();

				StreamingMipmapsMemoryBudget = reader.ReadSingle();
				StreamingMipmapsRenderersPerFrame = reader.ReadInt32();
				StreamingMipmapsMaxLevelReduction = reader.ReadInt32();
				StreamingMipmapsMaxFileIORequests = reader.ReadInt32();
			}
			if (HasParticleRaycastBudget(reader.Version))
			{
				ParticleRaycastBudget = reader.ReadInt32();
			}
			if (HasAsyncUploadTimeSlice(reader.Version))
			{
				AsyncUploadTimeSlice = reader.ReadInt32();
				AsyncUploadBufferSize = reader.ReadInt32();
			}
			if (HasAsyncUploadPersistentBuffer(reader.Version))
			{
				AsyncUploadPersistentBuffer = reader.ReadBoolean();
				reader.AlignStream();
			}
			if (HasResolutionScalingFixedDPIFactor(reader.Version))
			{
				ResolutionScalingFixedDPIFactor = reader.ReadSingle();
			}
			if (HasCustomRenderPipeline(reader.Version))
			{
				CustomRenderPipeline.Read(reader);
			}
			if (HasVSyncCount(reader.Version))
			{
				reader.AlignStream();
			}

#if UNIVERSAL
			if (HasExcludedTargetPlatforms(reader.Version, reader.Flags))
			{
				ExcludedTargetPlatforms = reader.ReadStringArray();
			}
#endif
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(NameName, Name);
			node.Add(PixelLightCountName, PixelLightCount);
			node.Add(ShadowsName, (int)Shadows);
			node.Add(ShadowResolutionName, (int)ShadowResolution);
			node.Add(ShadowProjectionName, (int)ShadowProjection);
			node.Add(ShadowCascadesName, (int)ShadowCascades);
			node.Add(ShadowDistanceName, ShadowDistance);
			node.Add(ShadowNearPlaneOffsetName, ShadowNearPlaneOffset);
			node.Add(ShadowCascade2SplitName, ShadowCascade2Split);
			node.Add(ShadowCascade4SplitName, ShadowCascade4Split.ExportYAML(container));
			node.Add(ShadowmaskModeName, (int)ShadowmaskMode);
			node.Add(GetSkinWeightsName(container.ExportVersion), (int)SkinWeights);
			node.Add(TextureQualityName, (int)TextureQuality);
			node.Add(AnisotropicTexturesName, (int)AnisotropicTextures);
			node.Add(AntiAliasingName, (int)AntiAliasing);
			node.Add(SoftParticlesName, SoftParticles);
			node.Add(SoftVegetationName, SoftVegetation);
			node.Add(RealtimeReflectionProbesName, RealtimeReflectionProbes);
			node.Add(BillboardsFaceCameraPositionName, BillboardsFaceCameraPosition);
			node.Add(VSyncCountName, (int)VSyncCount);
			node.Add(LodBiasName, LodBias);
			node.Add(MaximumLODLevelName, MaximumLODLevel);
			if (HasStreamingMipmapsActive(container.ExportVersion))
			{
				node.Add(StreamingMipmapsActiveName, StreamingMipmapsActive);
				node.Add(StreamingMipmapsAddAllCamerasName, GetStreamingMipmapsAddAllCameras(container.Version));
				node.Add(StreamingMipmapsMemoryBudgetName, GetStreamingMipmapsMemoryBudget(container.Version));
				node.Add(StreamingMipmapsRenderersPerFrameName, GetStreamingMipmapsRenderersPerFrame(container.Version));
				node.Add(StreamingMipmapsMaxLevelReductionName, GetStreamingMipmapsMaxLevelReduction(container.Version));
				node.Add(StreamingMipmapsMaxFileIORequestsName, GetStreamingMipmapsMaxFileIORequests(container.Version));
			}
			node.Add(ParticleRaycastBudgetName, ParticleRaycastBudget);
			node.Add(AsyncUploadTimeSliceName, AsyncUploadTimeSlice);
			node.Add(AsyncUploadBufferSizeName, AsyncUploadBufferSize);
			if (HasAsyncUploadPersistentBuffer(container.ExportVersion))
			{
				node.Add(AsyncUploadPersistentBufferName, AsyncUploadPersistentBuffer);
			}
			node.Add(ResolutionScalingFixedDPIFactorName, ResolutionScalingFixedDPIFactor);
			if (HasCustomRenderPipeline(container.ExportVersion))
			{
				node.Add(CustomRenderPipelineName, CustomRenderPipeline.ExportYAML(container));
			}
			node.Add(ExcludedTargetPlatformsName, GetExcludedTargetPlatforms(container.Version, container.Flags).ExportYAML());
			return node;
		}

		private IReadOnlyList<string> GetExcludedTargetPlatforms(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasExcludedTargetPlatforms(version, flags))
			{
				return ExcludedTargetPlatforms;
			}
#endif
			return System.Array.Empty<string>();
		}
		private bool GetStreamingMipmapsAddAllCameras(Version version)
		{
			return HasStreamingMipmapsActive(version) ? StreamingMipmapsAddAllCameras : true;
		}
		private float GetStreamingMipmapsMemoryBudget(Version version)
		{
			return HasStreamingMipmapsActive(version) ? StreamingMipmapsMemoryBudget : 512.0f;
		}
		private int GetStreamingMipmapsRenderersPerFrame(Version version)
		{
			return HasStreamingMipmapsActive(version) ? StreamingMipmapsRenderersPerFrame : 512;
		}
		private int GetStreamingMipmapsMaxLevelReduction(Version version)
		{
			return HasStreamingMipmapsActive(version) ? StreamingMipmapsMaxLevelReduction : 2;
		}
		private int GetStreamingMipmapsMaxFileIORequests(Version version)
		{
			return HasStreamingMipmapsActive(version) ? StreamingMipmapsMaxFileIORequests : 1024;
		}

		private string GetSkinWeightsName(Version version)
		{
			return version.IsGreaterEqual(2019) ? SkinWeightsName : BlendWeightsName;
		}

		public string Name { get; set; }
		public int PixelLightCount { get; set; }
		public ShadowQuality Shadows { get; set; }
		public ShadowResolution ShadowResolution { get; set; }
		public ShadowProjection ShadowProjection { get; set; }
		public ShadowCascades ShadowCascades { get; set; }
		public float ShadowDistance { get; set; }
		public float ShadowNearPlaneOffset { get; set; }
		public float ShadowCascade2Split { get; set; }
		public ShadowmaskMode ShadowmaskMode { get; set; }
		/// <summary>
		/// BlendWeights previously
		/// </summary>
		public SkinWeights SkinWeights { get; set; }
		public TextureQuality TextureQuality { get; set; }
		public AnisotropicFiltering AnisotropicTextures { get; set; }
		public AntiAliasing AntiAliasing { get; set; }
		public bool SoftParticles { get; set; }
		public bool SoftVegetation { get; set; }
		public bool RealtimeReflectionProbes { get; set; }
		public bool BillboardsFaceCameraPosition { get; set; }
		public bool SyncToVBL { get; set; }
		public VSyncCount VSyncCount { get; set; }
		public float LodBias { get; set; }
		public int MaximumLODLevel { get; set; }
		public bool StreamingMipmapsActive { get; set; }
		public bool StreamingMipmapsAddAllCameras { get; set; }
		public float StreamingMipmapsMemoryBudget { get; set; }
		public int StreamingMipmapsRenderersPerFrame { get; set; }
		public int StreamingMipmapsMaxLevelReduction { get; set; }
		public int StreamingMipmapsMaxFileIORequests { get; set; }
		public int ParticleRaycastBudget { get; set; }
		public int AsyncUploadTimeSlice { get; set; }
		public int AsyncUploadBufferSize { get; set; }
		public bool AsyncUploadPersistentBuffer { get; set; }
		public float ResolutionScalingFixedDPIFactor { get; set; }
#if UNIVERSAL
		public string[] ExcludedTargetPlatforms { get; set; }
#endif

		public const string NameName = "name";
		public const string PixelLightCountName = "pixelLightCount";
		public const string ShadowsName = "shadows";
		public const string ShadowResolutionName = "shadowResolution";
		public const string ShadowProjectionName = "shadowProjection";
		public const string ShadowCascadesName = "shadowCascades";
		public const string ShadowDistanceName = "shadowDistance";
		public const string ShadowNearPlaneOffsetName = "shadowNearPlaneOffset";
		public const string ShadowCascade2SplitName = "shadowCascade2Split";
		public const string ShadowCascade4SplitName = "shadowCascade4Split";
		public const string ShadowmaskModeName = "shadowmaskMode";
		public const string BlendWeightsName = "blendWeights";
		public const string SkinWeightsName = "skinWeights";
		public const string TextureQualityName = "textureQuality";
		public const string AnisotropicTexturesName = "anisotropicTextures";
		public const string AntiAliasingName = "antiAliasing";
		public const string SoftParticlesName = "softParticles";
		public const string SoftVegetationName = "softVegetation";
		public const string RealtimeReflectionProbesName = "realtimeReflectionProbes";
		public const string BillboardsFaceCameraPositionName = "billboardsFaceCameraPosition";
		public const string VSyncCountName = "vSyncCount";
		public const string LodBiasName = "lodBias";
		public const string MaximumLODLevelName = "maximumLODLevel";
		public const string StreamingMipmapsActiveName = "streamingMipmapsActive";
		public const string StreamingMipmapsAddAllCamerasName = "streamingMipmapsAddAllCameras";
		public const string StreamingMipmapsMemoryBudgetName = "streamingMipmapsMemoryBudget";
		public const string StreamingMipmapsRenderersPerFrameName = "streamingMipmapsRenderersPerFrame";
		public const string StreamingMipmapsMaxLevelReductionName = "streamingMipmapsMaxLevelReduction";
		public const string StreamingMipmapsMaxFileIORequestsName = "streamingMipmapsMaxFileIORequests";
		public const string ParticleRaycastBudgetName = "particleRaycastBudget";
		public const string AsyncUploadTimeSliceName = "asyncUploadTimeSlice";
		public const string AsyncUploadBufferSizeName = "asyncUploadBufferSize";
		public const string AsyncUploadPersistentBufferName = "asyncUploadPersistentBuffer";
		public const string ResolutionScalingFixedDPIFactorName = "resolutionScalingFixedDPIFactor";
		public const string CustomRenderPipelineName = "customRenderPipeline";
		public const string ExcludedTargetPlatformsName = "excludedTargetPlatforms";

		public Vector3f ShadowCascade4Split;
		public PPtr<MonoBehaviour> CustomRenderPipeline;
	}
}
