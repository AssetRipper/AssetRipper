using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.TierGraphicsSettings;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class TierGraphicsSettingsExtensions
	{
		public static RenderingPath GetRenderingPath(this ITierGraphicsSettings settings)
		{
			return (RenderingPath)settings.RenderingPath;
		}

		public static CameraHDRMode GetHdrMode(this ITierGraphicsSettings settings)
		{
			return (CameraHDRMode)settings.HdrMode;
		}

		public static RealtimeGICPUUsage GetRealtimeGICPUUsage(this ITierGraphicsSettings settings)
		{
			return (RealtimeGICPUUsage)settings.RealtimeGICPUUsage;
		}
	}
}
