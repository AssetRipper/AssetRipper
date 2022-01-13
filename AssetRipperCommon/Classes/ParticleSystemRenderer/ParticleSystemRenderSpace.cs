namespace AssetRipper.Core.Classes.ParticleSystemRenderer
{
	/// <summary>
	/// How particles are aligned when rendered.
	/// </summary>
	public enum ParticleSystemRenderSpace
	{
		/// <summary>
		/// Particles face the camera plane.
		/// </summary>
		View = 0,
		/// <summary>
		/// Particles align with the world.
		/// </summary>
		World = 1,
		/// <summary>
		/// Particles align with their local transform.
		/// </summary>
		Local = 2,
		/// <summary>
		/// Particles face the eye position.
		/// </summary>
		Facing = 3,
		/// <summary>
		/// Particles are aligned to their direction of travel.
		/// </summary>
		Velocity = 4,
	}
}
