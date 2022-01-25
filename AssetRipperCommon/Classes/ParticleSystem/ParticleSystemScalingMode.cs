namespace AssetRipper.Core.Classes.ParticleSystem
{
	/// <summary>
	/// Control how particle systems apply transform scale.
	/// </summary>
	public enum ParticleSystemScalingMode
	{
		/// <summary>
		/// Scale the particle system using the entire transform hierarchy.
		/// </summary>
		Hierarchy = 0,
		/// <summary>
		/// Scale the particle system using only its own transform scale. (Ignores parent scale).
		/// </summary>
		Local = 1,
		/// <summary>
		/// Only apply transform scale to the shape component, which controls where particles
		/// are spawned, but does not affect their size or movement.
		/// </summary>
		Shape = 2
	}
}
