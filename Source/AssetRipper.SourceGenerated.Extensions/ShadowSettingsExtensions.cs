using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.ShadowSettings;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ShadowSettingsExtensions
{
	public static LightShadows GetLightmapBakeType(this IShadowSettings settings)
	{
		return (LightShadows)settings.Type;
	}
}
