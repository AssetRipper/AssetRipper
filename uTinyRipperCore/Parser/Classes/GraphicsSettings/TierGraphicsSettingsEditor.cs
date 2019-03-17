using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Cameras;
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
		public static bool IsReadHdrMode(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadUseHDR(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadPrefer32BitShadowMaps(Version version)
		{
			return version.IsGreaterEqual(2017);
		}
		/// <summary>
		/// 5.6.3 and greater
		/// </summary>
		public static bool IsReadEnableLPPV(Version version)
		{
			return version.IsGreaterEqual(5, 6, 3);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadUseDitherMaskForAlphaBlendedShadows(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		private static bool IsReadUseCascadedShadowMapsFirst(Version version)
		{
			return version.IsLess(5, 6);
		}

		public void Read(AssetReader reader)
		{
			StandardShaderQuality = (ShaderQuality)reader.ReadInt32();
			RenderingPath = (RenderingPath)reader.ReadInt32();
			if (IsReadUseCascadedShadowMapsFirst(reader.Version))
			{
				UseCascadedShadowMaps = reader.ReadBoolean();
			}
			if (IsReadHdrMode(reader.Version))
			{
				HdrMode = (CameraHDRMode)reader.ReadInt32();
				RealtimeGICPUUsage = (RealtimeGICPUUsage)reader.ReadInt32();
			}
			UseReflectionProbeBoxProjection = reader.ReadBoolean();
			UseReflectionProbeBlending = reader.ReadBoolean();
			if (IsReadUseHDR(reader.Version))
			{
				UseHDR = reader.ReadBoolean();
				UseDetailNormalMap = reader.ReadBoolean();
			}
			if (!IsReadUseCascadedShadowMapsFirst(reader.Version))
			{
				UseCascadedShadowMaps = reader.ReadBoolean();
			}
			if (IsReadPrefer32BitShadowMaps(reader.Version))
			{
				Prefer32BitShadowMaps = reader.ReadBoolean();
			}
			if (IsReadEnableLPPV(reader.Version))
			{
				EnableLPPV = reader.ReadBoolean();
			}
			if (IsReadUseDitherMaskForAlphaBlendedShadows(reader.Version))
			{
				UseDitherMaskForAlphaBlendedShadows = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("standardShaderQuality", (int)StandardShaderQuality);
			node.Add("renderingPath", (int)RenderingPath);
			node.Add("hdrMode", (int)HdrMode);
			node.Add("realtimeGICPUUsage", (int)RealtimeGICPUUsage);
			node.Add("useReflectionProbeBoxProjection", UseReflectionProbeBoxProjection);
			node.Add("useReflectionProbeBlending", UseReflectionProbeBlending);
			node.Add("useHDR", UseHDR);
			node.Add("useDetailNormalMap", UseDetailNormalMap);
			node.Add("useCascadedShadowMaps", UseCascadedShadowMaps);
			node.Add("prefer32BitShadowMaps", Prefer32BitShadowMaps);
			node.Add("enableLPPV", EnableLPPV);
			node.Add("useDitherMaskForAlphaBlendedShadows", UseDitherMaskForAlphaBlendedShadows);
			return node;
		}

		public ShaderQuality StandardShaderQuality { get; private set; }
		public RenderingPath RenderingPath { get; private set; }
		public CameraHDRMode HdrMode { get; private set; }
		public RealtimeGICPUUsage RealtimeGICPUUsage { get; private set; }
		public bool UseReflectionProbeBoxProjection { get; private set; }
		public bool UseReflectionProbeBlending { get; private set; }
		public bool UseHDR { get; private set; }
		public bool UseDetailNormalMap { get; private set; }
		public bool UseCascadedShadowMaps { get; private set; }
		public bool Prefer32BitShadowMaps { get; private set; }
		public bool EnableLPPV { get; private set; }
		public bool UseDitherMaskForAlphaBlendedShadows { get; private set; }
	}
}
