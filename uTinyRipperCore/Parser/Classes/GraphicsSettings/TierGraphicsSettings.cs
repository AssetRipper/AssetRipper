using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Cameras;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.GraphicsSettingss
{
	public struct TierGraphicsSettings : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadHdrMode(Version version)
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
		public static bool IsReadUseHDR(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		public void Read(AssetReader reader)
		{
			RenderingPath = (RenderingPath)reader.ReadInt32();
			if (IsReadHdrMode(reader.Version))
			{
				HdrMode = (CameraHDRMode)reader.ReadInt32();
				RealtimeGICPUUsage = (RealtimeGICPUUsage)reader.ReadInt32();
			}
			UseCascadedShadowMaps = reader.ReadBoolean();
			if (IsReadPrefer32BitShadowMaps(reader.Version))
			{
				Prefer32BitShadowMaps = reader.ReadBoolean();
			}
			if (IsReadEnableLPPV(reader.Version))
			{
				EnableLPPV = reader.ReadBoolean();
			}
			if (IsReadUseHDR(reader.Version))
			{
				UseHDR = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("renderingPath", (int)RenderingPath);
			node.Add("hdrMode", (int)HdrMode);
			node.Add("realtimeGICPUUsage", (int)RealtimeGICPUUsage);
			node.Add("useCascadedShadowMaps", UseCascadedShadowMaps);
			node.Add("prefer32BitShadowMaps", Prefer32BitShadowMaps);
			node.Add("enableLPPV", EnableLPPV);
			node.Add("useHDR", UseHDR);
			return node;
		}

		public RenderingPath RenderingPath { get; private set; }
		public CameraHDRMode HdrMode { get; private set; }
		public RealtimeGICPUUsage RealtimeGICPUUsage { get; private set; }
		public bool UseCascadedShadowMaps { get; private set; }
		public bool Prefer32BitShadowMaps { get; private set; }
		public bool EnableLPPV { get; private set; }
		public bool UseHDR { get; private set; }
	}
}
