namespace uTinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// The type of collisions to use for a given particle system.
	/// </summary>
	public enum ParticleSystemCollisionType
	{
		/// <summary>
		/// Collide with a list of planes.
		/// </summary>
		Planes	= 0,
		/// <summary>
		/// Collide with the world geometry.
		/// </summary>
		World	= 1,
	}
}
