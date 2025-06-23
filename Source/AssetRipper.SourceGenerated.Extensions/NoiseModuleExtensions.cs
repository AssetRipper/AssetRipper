using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.NoiseModule;

namespace AssetRipper.SourceGenerated.Extensions;

public static class NoiseModuleExtensions
{
	public static void SetToDefaults(this INoiseModule module, UnityVersion version)
	{
		module.Strength.SetValues(version, 1.0f);
		module.StrengthY.SetValues(version, 1.0f);
		module.StrengthZ.SetValues(version, 1.0f);
		module.Frequency = 0.5f;
		module.Damping = true;
		module.Octaves = 1;
		module.OctaveMultiplier = 0.5f;
		module.OctaveScale = 2.0f;
		module.Quality = (int)ParticleSystemNoiseQuality.High;
		module.ScrollSpeed.SetValues(version, 0.0f);
		module.Remap.SetValues(version, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
		module.RemapY.SetValues(version, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
		module.RemapZ.SetValues(version, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
		module.PositionAmount?.SetValues(version, 1.0f);
		module.RotationAmount?.SetValues(version, 0.0f);
		module.SizeAmount?.SetValues(version, 0.0f);
	}

	public static ParticleSystemNoiseQuality GetQuality(this INoiseModule module)
	{
		return (ParticleSystemNoiseQuality)module.Quality;
	}
}
