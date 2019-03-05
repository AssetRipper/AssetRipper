namespace uTinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// Control how particles are removed from the Particle System.
	/// </summary>
	public enum ParticleSystemRingBufferMode
	{
		/// <summary>
		/// Particles are removed when their age exceeds their lifetime.
		/// </summary>
		Disabled			= 0,
		/// <summary>
		/// Particles are removed when creating new particles would exceed the Max Particles property.
		/// </summary>
		PauseUntilReplaced	= 1,
		/// <summary>
		/// Particles are removed when creating new particles would exceed the Max Particles property. Before being removed, particles remain alive until their age exceeds their lifetime.
		/// </summary>
		LoopUntilReplaced	= 2,
	}
}
