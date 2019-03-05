using System;

namespace uTinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// The properties of sub-emitter particles.
	/// </summary>
	[Flags]
	public enum ParticleSystemSubEmitterProperties
	{
		/// <summary>
		/// When spawning new particles, do not inherit any properties from the parent particles.
		/// </summary>
		InheritNothing		= 0,
		/// <summary>
		/// When spawning new particles, multiply the start color by the color of the parent particles.
		/// </summary>
		InheritColor		= 1,
		/// <summary>
		/// When spawning new particles, multiply the start size by the size of the parent particles.
		/// </summary>
		InheritSize			= 2,
		/// <summary>
		/// When spawning new particles, add the start rotation to the rotation of the parent particles.
		/// </summary>
		InheritRotation		= 4,
		/// <summary>
		/// New particles will have a shorter lifespan, the closer their parent particles are to death.
		/// </summary>
		InheritLifetime		= 8,
		/// <summary>
		/// When spawning new particles, use the duration and age properties from the parent system, when sampling Main module curves in the Sub-Emitter.
		/// </summary>
		InheritDuration		= 16,

		/// <summary>
		/// When spawning new particles, inherit all available properties from the parent particles.
		/// </summary>
		InheritEverything = 0x1F,
	}
}
