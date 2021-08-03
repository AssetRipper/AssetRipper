namespace AssetRipper.Core.Classes.ParticleSystem.TrailModule
{
	/// <summary>
	/// Choose how Particle Trails are generated.
	/// </summary>
	public enum ParticleSystemTrailMode
	{
		/// <summary>
		/// Makes a trail behind each particle as the particle moves.
		/// </summary>
		PerParticle = 0,
		/// <summary>
		/// Draws a line between each particle, connecting the youngest particle to the oldest.
		/// </summary>
		Ribbon = 1
	}
}
