using AssetRipper.SourceGenerated.Classes.ClassID_157;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class LightmapSettingsExtensions
{
	public static void ConvertToEditorFormat(this ILightmapSettings settings)
	{
		settings.LightmapEditorSettings.SetToDefault();
		if (settings.LightingSettingsP is { } lightingSettings)
		{
			settings.GIWorkflowMode = lightingSettings.GIWorkflowMode;
		}
		else
		{
			settings.GIWorkflowMode = (int)GIWorkflowMode.OnDemand;
		}
	}

	public static GIWorkflowMode GetGIWorkflowMode(this ILightmapSettings settings)
	{
		return (GIWorkflowMode)settings.GIWorkflowMode;
	}
}
