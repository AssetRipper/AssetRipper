using AssetRipper.SourceGenerated.Subclasses.LightsModule;

namespace AssetRipper.SourceGenerated.Extensions;

public static class LightsModuleExtensions
{
	public static void SetToDefault(this ILightsModule module, UnityVersion version)
	{
		module.RandomDistribution = true;
		module.Color = true;
		module.Range = true;
		module.Intensity = true;
		module.RangeCurve.SetValues(version, 1.0f);
		module.IntensityCurve.SetValues(version, 1.0f);
		module.MaxLights = 20;
	}
}
