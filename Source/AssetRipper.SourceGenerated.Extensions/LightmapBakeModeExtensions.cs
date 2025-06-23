using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.LightmapBakeMode;

namespace AssetRipper.SourceGenerated.Extensions;

public static class LightmapBakeModeExtensions
{
	public static LightmapBakeType GetLightmapBakeType(this ILightmapBakeMode light)
	{
		return (LightmapBakeType)light.LightmapBakeType;
	}
	public static NativeEnums.Global.MixedLightingMode GetMixedLightingMode(this ILightmapBakeMode light)
	{
		return (NativeEnums.Global.MixedLightingMode)light.MixedLightingMode;
	}
}
