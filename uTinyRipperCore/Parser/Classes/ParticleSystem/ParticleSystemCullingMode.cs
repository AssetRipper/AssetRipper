namespace uTinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// The action to perform when the Particle System is offscreen.
	/// </summary>
	public enum ParticleSystemCullingMode
	{
		/// <summary>
		/// For looping effects, the simulation is paused when offscreen, and for one-shot effects, the simulation will continue playing.
		/// </summary>
		Automatic		= 0,
		/// <summary>
		/// Pause the Particle System simulation when it is offscreen, and perform an extra simulation when the system comes back onscreen, creating the impression that it was never paused.
		/// </summary>
		PauseAndCatchup	= 1,
		/// <summary>
		/// Pause the Particle System simulation when it is offscreen.
		/// </summary>
		Pause			= 2,
		/// <summary>
		/// Continue simulating the Particle System when it is offscreen.
		/// </summary>
		AlwaysSimulate	= 3,
	}
}
