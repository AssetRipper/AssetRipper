using AssetRipper.Core.Classes.RenderSettings;
using AssetRipper.SourceGenerated.Classes.ClassID_104;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class RenderSettingsExtensions
	{
		public static FogMode GetFogMode(this IRenderSettings settings)
		{
			return (FogMode)settings.FogMode_C104;
		}

		public static AmbientMode GetAmbientMode(this IRenderSettings settings)
		{
			return (AmbientMode)settings.AmbientMode_C104;
		}
	}
}
