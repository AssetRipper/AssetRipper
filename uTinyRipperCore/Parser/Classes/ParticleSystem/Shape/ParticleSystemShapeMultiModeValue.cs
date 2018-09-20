namespace uTinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// The mode used to generate new points in a shape.
	/// </summary>
	public enum ParticleSystemShapeMultiModeValue
	{
		/// <summary>
		/// Generate points randomly.
		/// </summary>
		Random			= 0,
		/// <summary>
		/// Animate the emission point around the shape.
		/// </summary>
		Loop			= 1,
		/// <summary>
		/// Animate the emission point around the shape, alternating between clockwise and counter-clockwise directions.
		/// </summary>
		PingPong		= 2,
		/// <summary>
		/// Distribute new particles around the shape evenly.
		/// </summary>
		BurstSpread		= 3,
	}
}
