using AssetRipper.SourceGenerated.Classes.ClassID_198;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ParticleSystemExtensions
{
	public static ParticleSystemStopAction GetStopAction(this IParticleSystem system)
	{
		return (ParticleSystemStopAction)system.StopAction;
	}

	public static ParticleSystemCullingMode GetCullingMode(this IParticleSystem system)
	{
		return (ParticleSystemCullingMode)system.CullingMode;
	}

	public static ParticleSystemRingBufferMode GetRingBufferMode(this IParticleSystem system)
	{
		return (ParticleSystemRingBufferMode)system.RingBufferMode;
	}

	public static ParticleSystemSimulationSpace GetMoveWithTransform(this IParticleSystem system)
	{
		if (system.Has_MoveWithTransform_Boolean())
		{
			return system.MoveWithTransform_Boolean ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;
		}
		else
		{
			return (ParticleSystemSimulationSpace)system.MoveWithTransform_Int32;
		}
	}
}
