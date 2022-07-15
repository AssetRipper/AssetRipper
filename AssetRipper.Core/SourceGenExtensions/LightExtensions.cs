using AssetRipper.Core.Classes.Light;
using AssetRipper.SourceGenerated.Classes.ClassID_108;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class LightExtensions
	{
		public static LightType GetType(this ILight light)
		{
			return (LightType)light.Type_C108;
		}
		public static LightShape GetShape(this ILight light)
		{
			return (LightShape)light.Shape_C108;
		}
		public static LightRenderMode GetRenderMode(this ILight light)
		{
			return (LightRenderMode)light.RenderMode_C108;
		}
		public static LightmappingMode GetLightmapping(this ILight light)
		{
			return (LightmappingMode)light.Lightmapping_C108;
		}
		public static LightShadowCasterMode GetLightShadowCasterMode(this ILight light)
		{
			return (LightShadowCasterMode)light.LightShadowCasterMode_C108;
		}
	}
}
