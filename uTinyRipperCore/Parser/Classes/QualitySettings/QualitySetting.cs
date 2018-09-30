using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

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
			m_excludedTargetPlatforms = new string[0];
#endif
		}

		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadName(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadShadows(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool IsReadShadowProjection(Version version)
		{
			return version.IsGreaterEqual(3, 4);
		}
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadShadowCascades(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool IsReadShadowNearPlaneOffset(Version version)
		{
			return version.IsGreaterEqual(5, 2);
		}
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool IsReadShadowCascade2Split(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadShadowmaskMode(Version version)
		{
			return version.IsGreaterEqual(2017, 1);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadSoftParticles(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadSoftVegetation(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadRealtimeReflectionProbes(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool IsReadBillboardsFaceCameraPosition(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// Less than 3.4.0
		/// </summary>
		public static bool IsReadSyncToVBL(Version version)
		{
			return version.IsLess(3, 4);
		}
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool IsReadVSyncCount(Version version)
		{
			return version.IsGreaterEqual(3, 4);
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadLodBias(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadStreamingMipmapsActive(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadParticleRaycastBudget(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadAsyncUploadTimeSlice(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadResolutionScalingFixedDPIFactor(Version version)
		{
			return version.IsGreaterEqual(2017, 1);
		}
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool IsReadExcludedTargetPlatforms(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3, 5);
		}

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}
		
		private static int GetSerializedVersion(Version version)
		{
			// SyncToVBL has been removed
			if (Config.IsExportTopmostSerializedVersion || version.IsGreaterEqual(3, 4))
			{
				return 2;
			}
			return 1;
		}

		public void Merge(QualitySetting setting, Version version, TransferInstructionFlags flags)
		{
			if (!IsReadShadows(version))
			{
				Shadows = setting.Shadows;
				ShadowResolution = setting.ShadowResolution;
			}
			if (!IsReadShadowProjection(version))
			{
				ShadowProjection = setting.ShadowProjection;
			}
			if (!IsReadShadowCascades(version))
			{
				ShadowCascades = setting.ShadowCascades;
				ShadowDistance = setting.ShadowDistance;
			}
			if (!IsReadShadowNearPlaneOffset(version))
			{
				ShadowNearPlaneOffset = setting.ShadowNearPlaneOffset;
			}
			if (!IsReadShadowCascade2Split(version))
			{
				ShadowCascade2Split = setting.ShadowCascade2Split;
				ShadowCascade4Split = setting.ShadowCascade4Split;
			}
			if (!IsReadShadowmaskMode(version))
			{
				ShadowmaskMode = setting.ShadowmaskMode;
			}
			if (!IsReadSoftParticles(version))
			{
				SoftParticles = setting.SoftParticles;
			}
			if (!IsReadSoftVegetation(version))
			{
				SoftVegetation = setting.SoftVegetation;
			}
			if (!IsReadRealtimeReflectionProbes(version))
			{
				RealtimeReflectionProbes = setting.RealtimeReflectionProbes;
			}
			if (!IsReadBillboardsFaceCameraPosition(version))
			{
				BillboardsFaceCameraPosition = setting.BillboardsFaceCameraPosition;
			}
			if (!IsReadSyncToVBL(version))
			{
				SyncToVBL = setting.SyncToVBL;
			}
			if (!IsReadVSyncCount(version))
			{
				VSyncCount = setting.VSyncCount;
			}
			if (!IsReadLodBias(version))
			{
				LodBias = setting.LodBias;
				MaximumLODLevel = setting.MaximumLODLevel;
			}
			if (!IsReadStreamingMipmapsActive(version))
			{
				StreamingMipmapsActive = setting.StreamingMipmapsActive;
				StreamingMipmapsAddAllCameras = setting.StreamingMipmapsAddAllCameras;
				StreamingMipmapsMemoryBudget = setting.StreamingMipmapsMemoryBudget;
				StreamingMipmapsRenderersPerFrame = setting.StreamingMipmapsRenderersPerFrame;
				StreamingMipmapsMaxLevelReduction = setting.StreamingMipmapsMaxLevelReduction;
				StreamingMipmapsMaxFileIORequests = setting.StreamingMipmapsMaxFileIORequests;
			}
			if (!IsReadParticleRaycastBudget(version))
			{
				ParticleRaycastBudget = setting.ParticleRaycastBudget;
			}
			if (!IsReadAsyncUploadTimeSlice(version))
			{
				AsyncUploadTimeSlice = setting.AsyncUploadTimeSlice;
				AsyncUploadBufferSize = setting.AsyncUploadBufferSize;
			}
			if (!IsReadResolutionScalingFixedDPIFactor(version))
			{
				ResolutionScalingFixedDPIFactor = setting.ResolutionScalingFixedDPIFactor;
			}
#if UNIVERSAL
			if (!IsReadExcludedTargetPlatforms(version, flags))
			{
				m_excludedTargetPlatforms = setting.m_excludedTargetPlatforms;
			}
#endif
		}

		public void Read(AssetReader reader)
		{
			if (IsReadName(reader.Version))
			{
				Name = reader.ReadString();
			}
			PixelLightCount = reader.ReadInt32();
			if (IsReadShadows(reader.Version))
			{
				Shadows = (ShadowQuality)reader.ReadInt32();
				ShadowResolution = (ShadowResolution)reader.ReadInt32();
			}
			if (IsReadShadowProjection(reader.Version))
			{
				ShadowProjection = (ShadowProjection)reader.ReadInt32();
			}
			if (IsReadShadowCascades(reader.Version))
			{
				ShadowCascades = (ShadowCascades)reader.ReadInt32();
				ShadowDistance = reader.ReadSingle();
			}
			if (IsReadShadowNearPlaneOffset(reader.Version))
			{
				ShadowNearPlaneOffset = reader.ReadSingle();
			}
			if (IsReadShadowCascade2Split(reader.Version))
			{
				ShadowCascade2Split = reader.ReadSingle();
				ShadowCascade4Split.Read(reader);
			}
			if (IsReadShadowmaskMode(reader.Version))
			{
				ShadowmaskMode = (ShadowmaskMode)reader.ReadInt32();
			}

			BlendWeights = (BlendWeights)reader.ReadInt32();
			TextureQuality = (TextureQuality)reader.ReadInt32();
			AnisotropicTextures = (AnisotropicFiltering)reader.ReadInt32();
			AntiAliasing = (AntiAliasing)reader.ReadInt32();
			if (IsReadSoftParticles(reader.Version))
			{
				SoftParticles = reader.ReadBoolean();
			}
			if (IsReadSoftVegetation(reader.Version))
			{
				SoftVegetation = reader.ReadBoolean();
			}
			if (IsReadRealtimeReflectionProbes(reader.Version))
			{
				RealtimeReflectionProbes = reader.ReadBoolean();
			}
			if (IsReadBillboardsFaceCameraPosition(reader.Version))
			{
				BillboardsFaceCameraPosition = reader.ReadBoolean();
			}
			if (IsReadSyncToVBL(reader.Version))
			{
				SyncToVBL = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadVSyncCount(reader.Version))
			{
				VSyncCount = (VSyncCount)reader.ReadInt32();
			}
			if (IsReadLodBias(reader.Version))
			{
				LodBias = reader.ReadSingle();
				MaximumLODLevel = reader.ReadInt32();
			}

			if (IsReadStreamingMipmapsActive(reader.Version))
			{
				StreamingMipmapsActive = reader.ReadBoolean();
				StreamingMipmapsAddAllCameras = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);

				StreamingMipmapsMemoryBudget = reader.ReadSingle();
				StreamingMipmapsRenderersPerFrame = reader.ReadInt32();
				StreamingMipmapsMaxLevelReduction = reader.ReadInt32();
				StreamingMipmapsMaxFileIORequests = reader.ReadInt32();
			}
			if (IsReadParticleRaycastBudget(reader.Version))
			{
				ParticleRaycastBudget = reader.ReadInt32();
			}
			if (IsReadAsyncUploadTimeSlice(reader.Version))
			{
				AsyncUploadTimeSlice = reader.ReadInt32();
				AsyncUploadBufferSize = reader.ReadInt32();
			}
			if (IsReadResolutionScalingFixedDPIFactor(reader.Version))
			{
				ResolutionScalingFixedDPIFactor = reader.ReadSingle();
			}
			if (IsReadVSyncCount(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

#if UNIVERSAL
			if (IsReadExcludedTargetPlatforms(reader.Version, reader.Flags))
			{
				m_excludedTargetPlatforms = reader.ReadStringArray();
			}
#endif
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("name", Name);
			node.Add("pixelLightCount", PixelLightCount);
			node.Add("shadows", (int)Shadows);
			node.Add("shadowResolution", (int)ShadowResolution);
			node.Add("shadowProjection", (int)ShadowProjection);
			node.Add("shadowCascades", (int)ShadowCascades);
			node.Add("shadowDistance", ShadowDistance);
			node.Add("shadowNearPlaneOffset", ShadowNearPlaneOffset);
			node.Add("shadowCascade2Split", ShadowCascade2Split);
			node.Add("shadowCascade4Split", ShadowCascade4Split.ExportYAML(container));
			node.Add("shadowmaskMode", (int)ShadowmaskMode);
			node.Add("blendWeights", (int)BlendWeights);
			node.Add("textureQuality", (int)TextureQuality);
			node.Add("anisotropicTextures", (int)AnisotropicTextures);
			node.Add("antiAliasing", (int)AntiAliasing);
			node.Add("softParticles", SoftParticles);
			node.Add("softVegetation", SoftVegetation);
			node.Add("realtimeReflectionProbes", RealtimeReflectionProbes);
			node.Add("billboardsFaceCameraPosition", BillboardsFaceCameraPosition);
			node.Add("vSyncCount", (int)VSyncCount);
			node.Add("lodBias", LodBias);
			node.Add("maximumLODLevel", MaximumLODLevel);
			// 2018
			//node.Add("streamingMipmapsActive", StreamingMipmapsActive);
			//node.Add("streamingMipmapsAddAllCameras", StreamingMipmapsAddAllCameras);
			//node.Add("streamingMipmapsMemoryBudget", StreamingMipmapsMemoryBudget);
			//node.Add("streamingMipmapsRenderersPerFrame", StreamingMipmapsRenderersPerFrame);
			//node.Add("streamingMipmapsMaxLevelReduction", StreamingMipmapsMaxLevelReduction);
			//node.Add("streamingMipmapsMaxFileIORequests", StreamingMipmapsMaxFileIORequests);
			node.Add("particleRaycastBudget", ParticleRaycastBudget);
			node.Add("asyncUploadTimeSlice", AsyncUploadTimeSlice);
			node.Add("asyncUploadBufferSize", AsyncUploadBufferSize);
			node.Add("resolutionScalingFixedDPIFactor", ResolutionScalingFixedDPIFactor);
			node.Add("excludedTargetPlatforms", GetExcludedTargetPlatforms(container.Version, container.Flags).ExportYAML());
			return node;
		}

		private IReadOnlyList<string> GetExcludedTargetPlatforms(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if(IsReadExcludedTargetPlatforms(version, flags))
			{
				return ExcludedTargetPlatforms;
			}
#endif
			return new string[0];
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
		public BlendWeights BlendWeights { get; set; }
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
		public float ResolutionScalingFixedDPIFactor { get; set; }
#if UNIVERSAL
		public IReadOnlyList<string> ExcludedTargetPlatforms => m_excludedTargetPlatforms;
#endif

		public Vector3f ShadowCascade4Split;

#if UNIVERSAL
		private string[] m_excludedTargetPlatforms;
#endif
	}
}
