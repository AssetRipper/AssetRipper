using AssetRipper.Core.Classes.Camera;
using AssetRipper.Core.Classes.GraphicsSettings;
using AssetRipper.SourceGenerated.Subclasses.TierGraphicsSettings;

namespace AssetRipper.Core.SourceGenExtensions
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
