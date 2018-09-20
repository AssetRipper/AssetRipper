using System;

namespace uTinyRipper.Classes.ParticleSystems
{
	[Flags]
	public enum ParticleSystemSubEmitterProperties
	{
		InheritNothing		= 0,
		InheritColor		= 1,
		InheritSize			= 2,
		InheritRotation		= 4,
		InheritLifetime		= 8,

		InheritEverything	= 0xF,
	}
}
