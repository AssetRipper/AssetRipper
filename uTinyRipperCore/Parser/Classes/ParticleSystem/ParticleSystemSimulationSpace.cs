namespace uTinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// The space to simulate particles in.
	/// </summary>
	public enum ParticleSystemSimulationSpace
	{
		/// <summary>
		/// Simulate particles in local space.
		/// </summary>
		Local	= 0,
		/// <summary>
		/// Simulate particles in world space.
		/// </summary>
		World	 = 1,
		/// <summary>
		/// Simulate particles relative to a custom transform component, defined by ParticleSystem.MainModule.customSimulationSpace.
		/// </summary>
		Custom	= 2,
	}
}
