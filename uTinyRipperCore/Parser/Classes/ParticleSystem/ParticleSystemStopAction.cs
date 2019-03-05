namespace uTinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// The action to perform when the Particle System stops.
	/// </summary>
	public enum ParticleSystemStopAction
	{
		/// <summary>
		/// Do nothing.
		/// </summary>
		None		= 0,
		/// <summary>
		/// Disable the GameObject containing the Particle System.
		/// </summary>
		Disable		= 1,
		/// <summary>
		/// Destroy the GameObject containing the Particle System.
		/// </summary>
		Destroy		= 2,
		/// <summary>
		/// Call OnParticleSystemStopped on the ParticleSystem script.
		/// </summary>
		Callback	= 3,
	}
}
