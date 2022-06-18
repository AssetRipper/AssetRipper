using AssetRipper.Core.Classes.LightmapSettings;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.SourceGenerated.Classes.ClassID_157;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class LightmapSettingsExtensions
	{
		public static void ConvertToEditorFormat(this ILightmapSettings settings)
		{
			settings.GIWorkflowMode_C157 = (int)GIWorkflowMode.OnDemand;
			settings.LightmapEditorSettings_C157.SetToDefault();
		}

		public static GIWorkflowMode GetGIWorkflowMode(this ILightmapSettings settings)
		{
			return (GIWorkflowMode)settings.GIWorkflowMode_C157;
		}

		public static LightmapsMode GetLightmapsMode(this ILightmapSettings settings)
		{
			return (LightmapsMode)settings.LightmapsMode_C157;
		}

		public static ColorSpace GetBakedColorSpace(this ILightmapSettings settings)
		{
			return (ColorSpace)settings.BakedColorSpace_C157;
		}
	}
}
