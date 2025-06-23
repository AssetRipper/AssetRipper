using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.UVModule;

namespace AssetRipper.SourceGenerated.Extensions;

public static class UVModuleExtensions
{
	public static ParticleSystemAnimationRowMode GetRowMode(this IUVModule module)
	{
		if (module.Has_RowMode())
		{
			return (ParticleSystemAnimationRowMode)module.RowMode;
		}
		else
		{
			return module.RandomRow ? ParticleSystemAnimationRowMode.Random : ParticleSystemAnimationRowMode.Custom;
		}
	}

	public static bool GetRandomRow(this IUVModule module)
	{
		return module.RandomRow || module.RowMode == (int)ParticleSystemAnimationRowMode.Random;
	}

	public static void SetRandomRow(this IUVModule module, bool value)
	{
		if (module.Has_RandomRow())
		{
			module.RandomRow = value;
		}
		else
		{
			module.RowMode = (int)(value ? ParticleSystemAnimationRowMode.Random : ParticleSystemAnimationRowMode.Custom);
		}
	}
}
