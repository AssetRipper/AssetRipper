using AssetRipper.SourceGenerated.Classes.ClassID_108;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class LightExtensions
	{
		public static LightmappingMode GetLightmapping(this ILight light)
		{
			return (LightmappingMode)light.Lightmapping_C108;
		}
	}
}
