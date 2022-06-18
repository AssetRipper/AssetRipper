using AssetRipper.Core.Classes.ParticleSystem.Emission;
using AssetRipper.SourceGenerated.Subclasses.EmissionModule;
using AssetRipper.SourceGenerated.Subclasses.MinMaxCurve;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class EmissionModuleExtensions
	{
		public static IMinMaxCurve? GetRateOverTime(this IEmissionModule module)
		{
			if (module.Has_RateOverTime())
			{
				return module.RateOverTime;
			}
			else if (module.Has_Type() && module.Type == (int)EmissionType.Time)
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
			else if (module.Has_Type() && module.Type == (int)EmissionType.Distance)
			{
				return module.Rate;
			}
			else
			{
				return null;
			}
		}
	}
}
