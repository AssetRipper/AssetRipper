using AssetRipper.Core.Classes.ParticleSystem.UV;
using AssetRipper.SourceGenerated.Subclasses.UVModule;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class UVModuleExtensions
	{
		public static ParticleSystemAnimationMode GetMode(this IUVModule module)
		{
			return (ParticleSystemAnimationMode)module.Mode;
		}

		public static ParticleSystemAnimationTimeMode GetTimeMode(this IUVModule module)
		{
			return (ParticleSystemAnimationTimeMode)module.TimeMode;
		}

		public static ParticleSystemAnimationType GetAnimationType(this IUVModule module)
		{
			return (ParticleSystemAnimationType)module.AnimationType;
		}

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
}
