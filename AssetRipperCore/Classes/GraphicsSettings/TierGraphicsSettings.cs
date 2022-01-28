using AssetRipper.Core.Classes.Camera;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.GraphicsSettings
{
	public sealed class TierGraphicsSettings : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasHdrMode(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 5.6.0b7 and greater
		/// </summary>
		public static bool HasRealtimeGICPUUsage(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 7);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasPrefer32BitShadowMaps(UnityVersion version) => version.IsGreaterEqual(2017);
		/// <summary>
		/// 5.6.3 and greater
		/// </summary>
		public static bool HasEnableLPPV(UnityVersion version) => version.IsGreaterEqual(5, 6, 3);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasUseHDR(UnityVersion version) => version.IsGreaterEqual(5, 6);

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
