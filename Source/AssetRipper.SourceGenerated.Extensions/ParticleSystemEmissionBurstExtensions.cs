using AssetRipper.SourceGenerated.Subclasses.ParticleSystemEmissionBurst;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ParticleSystemEmissionBurstExtensions
{
	public static void SetValues(this IParticleSystemEmissionBurst burst, UnityVersion version, float time, int minValue, int maxValue)
	{
		burst.Time = time;
		burst.CycleCount_Int32 = 1;
		burst.CycleCount_UInt32 = 1;
		burst.RepeatInterval = 0.01f;
		burst.CountCurve?.SetValues(version, minValue, maxValue);
		burst.Probability = 1.0f;
	}
}
