using AssetRipper.Core.Classes.LightmapSettings.GISettings;
using AssetRipper.SourceGenerated.Subclasses.GISettings;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class GISettingsExtensions
	{
		public static void Initialize(this IGISettings settings)
		{
			settings.BounceScale = 1.0f;
			settings.IndirectOutputScale = 1.0f;
			settings.AlbedoBoost = 1.0f;
			settings.TemporalCoherenceThreshold = 1.0f;
			settings.EnvironmentLightingMode = (uint)EnvironmentAmbientMode.Realtime;
			settings.EnableBakedLightmaps = true;
			settings.EnableRealtimeLightmaps = true;
		}

		public static EnvironmentAmbientMode GetEnvironmentLightingMode(this IGISettings settings)
		{
			return (EnvironmentAmbientMode)settings.EnvironmentLightingMode;
		}
	}
}
