namespace uTinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// Control how animation frames are selected.
	/// </summary>
	public enum ParticleSystemAnimationTimeMode
	{
		/// <summary>
		/// Select animation frames based on the particle ages.
		/// </summary>
		Lifetime	= 0,
		/// <summary>
		/// Select animation frames based on the particle speeds.
		/// </summary>
		Speed		= 1,
		/// <summary>
		/// Select animation frames sequentially at a constant rate of the specified frames per second.
		/// </summary>
		FPS			= 2,
	}
}
