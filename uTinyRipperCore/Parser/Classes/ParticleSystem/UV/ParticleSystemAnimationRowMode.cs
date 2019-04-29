namespace uTinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// The mode used for selecting rows of an animation in the Texture Sheet Animation Module
	/// </summary>
	public enum ParticleSystemAnimationRowMode
	{
		/// <summary>
		/// Use a specific row for all particles
		/// </summary>
		Custom		= 0,
		/// <summary>
		/// Use a random row for each particle
		/// </summary>
		Random		= 1,
		/// <summary>
		/// Use the mesh index as the row, so that meshes can be mapped to specific animation frames
		/// </summary>
		MeshIndex	= 2,
	}
}
