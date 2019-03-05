namespace uTinyRipper.Classes.ParticleSystems
{
	/// <summary>
	/// Choose how textures are applied to Particle Trails.
	/// </summary>
	public enum ParticleSystemTrailTextureMode
	{
		/// <summary>
		/// Map the texture once along the entire length of the trail.
		/// </summary>
		Stretch					= 0,
		/// <summary>
		/// Repeat the texture along the trail. To set the tiling rate, use Material.SetTextureScale.
		/// </summary>
		Tile					= 1,
		/// <summary>
		/// Map the texture once along the entire length of the trail, assuming all vertices are evenly spaced.
		/// </summary>
		DistributePerSegment	= 2,
		/// <summary>
		/// Repeat the texture along the trail, repeating at a rate of once per trail segment. To adjust the tiling rate, use Material.SetTextureScale.
		/// </summary>
		RepeatPerSegment		= 3,
	}
}
