using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.ShadowSettings;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ShadowSettingsExtensions
	{
		public static LightShadows GetLightmapBakeType(this IShadowSettings settings)
		{
			return (LightShadows)settings.Type;
		}
	}
}
