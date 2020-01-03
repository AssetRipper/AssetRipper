using uTinyRipper.Classes.Cameras;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.GraphicsSettingss
{
	public struct TierGraphicsSettingsEditor : IAssetReadable, IYAMLExportable
	{
		public TierGraphicsSettingsEditor(PlatformShaderSettings settings, Version version, TransferInstructionFlags flags)
		{
			StandardShaderQuality = settings.GetStandardShaderQuality(version, flags);
			RenderingPath = RenderingPath.Forward;
			HdrMode = CameraHDRMode.FP16;
			RealtimeGICPUUsage = RealtimeGICPUUsage.Low;
			UseReflectionProbeBoxProjection = settings.GetUseReflectionProbeBoxProjection(version, flags);
			UseReflectionProbeBlending = settings.GetUseReflectionProbeBlending(version, flags);
			UseHDR = true;
			UseDetailNormalMap = true;
			UseCascadedShadowMaps = true;
			Prefer32BitShadowMaps = false;
			EnableLPPV = true;
			UseDitherMaskForAlphaBlendedShadows = true;
		}

		public TierGraphicsSettingsEditor(TierGraphicsSettings settings)
		{
			StandardShaderQuality = ShaderQuality.High;
			RenderingPath = settings.RenderingPath;
			HdrMode = settings.HdrMode;
			RealtimeGICPUUsage = settings.RealtimeGICPUUsage;
			UseReflectionProbeBoxProjection = true;
			UseReflectionProbeBlending = true;
			UseHDR = settings.UseHDR;
			UseDetailNormalMap = true;
			UseCascadedShadowMaps = settings.UseCascadedShadowMaps;
			Prefer32BitShadowMaps = settings.Prefer32BitShadowMaps;
			EnableLPPV = settings.EnableLPPV;
			UseDitherMaskForAlphaBlendedShadows = true;
		}

		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasHdrMode(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasUseHDR(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasPrefer32BitShadowMaps(Version version) => version.IsGreaterEqual(2017);
		/// <summary>
		/// 5.6.3 and greater
		/// </summary>
		public static bool HasEnableLPPV(Version version) => version.IsGreaterEqual(5, 6, 3);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasUseDitherMaskForAlphaBlendedShadows(Version version) => version.IsGreaterEqual(5, 6);

		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		private static bool HasUseCascadedShadowMapsFirst(Version version)
		{
			return version.IsLess(5, 6);
		}

		public void Read(AssetReader reader)
		{
			StandardShaderQuality = (ShaderQuality)reader.ReadInt32();
			RenderingPath = (RenderingPath)reader.ReadInt32();
			if (HasUseCascadedShadowMapsFirst(reader.Version))
			{
				UseCascadedShadowMaps = reader.ReadBoolean();
			}
			if (HasHdrMode(reader.Version))
			{
				HdrMode = (CameraHDRMode)reader.ReadInt32();
				RealtimeGICPUUsage = (RealtimeGICPUUsage)reader.ReadInt32();
			}
			UseReflectionProbeBoxProjection = reader.ReadBoolean();
			UseReflectionProbeBlending = reader.ReadBoolean();
			if (HasUseHDR(reader.Version))
			{
				UseHDR = reader.ReadBoolean();
				UseDetailNormalMap = reader.ReadBoolean();
			}
			if (!HasUseCascadedShadowMapsFirst(reader.Version))
			{
				UseCascadedShadowMaps = reader.ReadBoolean();
			}
			if (HasPrefer32BitShadowMaps(reader.Version))
			{
				Prefer32BitShadowMaps = reader.ReadBoolean();
			}
			if (HasEnableLPPV(reader.Version))
			{
				EnableLPPV = reader.ReadBoolean();
			}
			if (HasUseDitherMaskForAlphaBlendedShadows(reader.Version))
			{
				UseDitherMaskForAlphaBlendedShadows = reader.ReadBoolean();
			}
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(StandardShaderQualityName, (int)StandardShaderQuality);
			node.Add(RenderingPathName, (int)RenderingPath);
			node.Add(HdrModeName, (int)HdrMode);
			node.Add(RealtimeGICPUUsageName, (int)RealtimeGICPUUsage);
			node.Add(UseReflectionProbeBoxProjectionName, UseReflectionProbeBoxProjection);
			node.Add(UseReflectionProbeBlendingName, UseReflectionProbeBlending);
			node.Add(UseHDRName, UseHDR);
			node.Add(UseDetailNormalMapName, UseDetailNormalMap);
			node.Add(UseCascadedShadowMapsName, UseCascadedShadowMaps);
			node.Add(Prefer32BitShadowMapsName, Prefer32BitShadowMaps);
			node.Add(EnableLPPVName, EnableLPPV);
			node.Add(UseDitherMaskForAlphaBlendedShadowsName, UseDitherMaskForAlphaBlendedShadows);
			return node;
		}

		public ShaderQuality StandardShaderQuality { get; set; }
		public RenderingPath RenderingPath { get; set; }
		public CameraHDRMode HdrMode { get; set; }
		public RealtimeGICPUUsage RealtimeGICPUUsage { get; set; }
		public bool UseReflectionProbeBoxProjection { get; set; }
		public bool UseReflectionProbeBlending { get; set; }
		public bool UseHDR { get; set; }
		public bool UseDetailNormalMap { get; set; }
		public bool UseCascadedShadowMaps { get; set; }
		public bool Prefer32BitShadowMaps { get; set; }
		public bool EnableLPPV { get; set; }
		public bool UseDitherMaskForAlphaBlendedShadows { get; set; }

		public const string StandardShaderQualityName = "standardShaderQuality";
		public const string RenderingPathName = "renderingPath";
		public const string HdrModeName = "hdrMode";
		public const string RealtimeGICPUUsageName = "realtimeGICPUUsage";
		public const string UseReflectionProbeBoxProjectionName = "useReflectionProbeBoxProjection";
		public const string UseReflectionProbeBlendingName = "useReflectionProbeBlending";
		public const string UseHDRName = "useHDR";
		public const string UseDetailNormalMapName = "useDetailNormalMap";
		public const string UseCascadedShadowMapsName = "useCascadedShadowMaps";
		public const string Prefer32BitShadowMapsName = "prefer32BitShadowMaps";
		public const string EnableLPPVName = "enableLPPV";
		public const string UseDitherMaskForAlphaBlendedShadowsName = "useDitherMaskForAlphaBlendedShadows";
	}
}
