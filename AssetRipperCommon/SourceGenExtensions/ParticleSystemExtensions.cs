using AssetRipper.Core.Classes.ParticleSystem;
using AssetRipper.SourceGenerated.Classes.ClassID_198;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ParticleSystemExtensions
	{
		public static ParticleSystemStopAction GetStopAction(this IParticleSystem system)
		{
			return (ParticleSystemStopAction)system.StopAction_C198;
		}

		public static ParticleSystemCullingMode GetCullingMode(this IParticleSystem system)
		{
			return (ParticleSystemCullingMode)system.CullingMode_C198;
		}

		public static ParticleSystemRingBufferMode GetRingBufferMode(this IParticleSystem system)
		{
			return (ParticleSystemRingBufferMode)system.RingBufferMode_C198;
		}

		public static ParticleSystemSimulationSpace GetMoveWithTransform(this IParticleSystem system)
		{
			if (system.Has_MoveWithTransform_C198_Boolean())
			{
				return system.MoveWithTransform_C198_Boolean ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;
			}
			else
			{
				return (ParticleSystemSimulationSpace)system.MoveWithTransform_C198_Int32;
			}
		}

		public static ParticleSystemScalingMode GetScalingMode(this IParticleSystem system)
		{
			return (ParticleSystemScalingMode)system.ScalingMode_C198;
		}
	}
}
