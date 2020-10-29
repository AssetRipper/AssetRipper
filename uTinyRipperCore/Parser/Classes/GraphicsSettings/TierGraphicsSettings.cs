using uTinyRipper.Classes.Cameras;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.GraphicsSettingss
{
	public struct TierGraphicsSettings : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasHdrMode(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 5.6.0b7 and greater
		/// </summary>
		public static bool HasRealtimeGICPUUsage(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 7);
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
		public static bool HasUseHDR(Version version) => version.IsGreaterEqual(5, 6);

		public void Read(AssetReader reader)
		{
			RenderingPath = (RenderingPath)reader.ReadInt32();
			if (HasHdrMode(reader.Version))
			{
				HdrMode = (CameraHDRMode)reader.ReadInt32();
			}
			if (HasRealtimeGICPUUsage(reader.Version))
			{
				RealtimeGICPUUsage = (RealtimeGICPUUsage)reader.ReadInt32();
			}
			UseCascadedShadowMaps = reader.ReadBoolean();
			if (HasPrefer32BitShadowMaps(reader.Version))
			{
				Prefer32BitShadowMaps = reader.ReadBoolean();
			}
			if (HasEnableLPPV(reader.Version))
			{
				EnableLPPV = reader.ReadBoolean();
			}
			if (HasUseHDR(reader.Version))
			{
				UseHDR = reader.ReadBoolean();
			}
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(RenderingPathName, (int)RenderingPath);
			node.Add(HdrModeName, (int)HdrMode);
			node.Add(RealtimeGICPUUsageName, (int)RealtimeGICPUUsage);
			node.Add(UseCascadedShadowMapsName, UseCascadedShadowMaps);
			node.Add(Prefer32BitShadowMapsName, Prefer32BitShadowMaps);
			node.Add(EnableLPPVName, EnableLPPV);
			node.Add(UseHDRName, UseHDR);
			return node;
		}

		public RenderingPath RenderingPath { get; set; }
		public CameraHDRMode HdrMode { get; set; }
		public RealtimeGICPUUsage RealtimeGICPUUsage { get; set; }
		public bool UseCascadedShadowMaps { get; set; }
		public bool Prefer32BitShadowMaps { get; set; }
		public bool EnableLPPV { get; set; }
		public bool UseHDR { get; set; }

		public const string RenderingPathName = "renderingPath";
		public const string HdrModeName = "hdrMode";
		public const string RealtimeGICPUUsageName = "realtimeGICPUUsage";
		public const string UseCascadedShadowMapsName = "useCascadedShadowMaps";
		public const string Prefer32BitShadowMapsName = "prefer32BitShadowMaps";
		public const string EnableLPPVName = "enableLPPV";
		public const string UseHDRName = "useHDR";
	}
}
