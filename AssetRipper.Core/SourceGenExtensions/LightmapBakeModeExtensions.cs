using AssetRipper.Core.Classes.Light;
using AssetRipper.SourceGenerated.Subclasses.LightmapBakeMode;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class LightmapBakeModeExtensions
	{
		public static LightmapBakeType GetLightmapBakeType(this ILightmapBakeMode light)
		{
			return (LightmapBakeType)light.LightmapBakeType;
		}
		public static MixedLightingMode GetMixedLightingMode(this ILightmapBakeMode light)
		{
			return (MixedLightingMode)light.MixedLightingMode;
		}
	}
}
