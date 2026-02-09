using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.EmissionModule;
using AssetRipper.SourceGenerated.Subclasses.MinMaxCurve;

namespace AssetRipper.SourceGenerated.Extensions;

public static class EmissionModuleExtensions
{
	public static IMinMaxCurve? GetRateOverTime(this IEmissionModule module)
	{
		if (module.Has_RateOverTime())
		{
			return module.RateOverTime;
		}
		else if (module.Has_Type() && module.TypeE == ParticleSystemEmissionType.Time)
		{
			return module.Rate;
		}
		else
		{
			return null;
		}
	}

	public static IMinMaxCurve? GetRateOverDistance(this IEmissionModule module)
	{
		if (module.Has_RateOverDistance())
		{
			return module.RateOverDistance;
		}
		else if (module.Has_Type() && module.TypeE == ParticleSystemEmissionType.Distance)
		{
			return module.Rate;
		}
		else
		{
			return null;
		}
	}
}
