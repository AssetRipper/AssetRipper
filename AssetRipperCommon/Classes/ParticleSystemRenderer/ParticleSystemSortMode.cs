namespace AssetRipper.Core.Classes.ParticleSystemRenderer
{
	/// <summary>
	/// The sorting mode for particle systems.
	/// </summary>
	public enum ParticleSystemSortMode
	{
		/// <summary>
		/// No sorting.
		/// </summary>
		None = 0,
		/// <summary>
		/// Sort based on distance.
		/// </summary>
		Distance = 1,
		/// <summary>
		/// Sort the oldest particles to the front.
		/// </summary>
		OldestInFront = 2,
		/// <summary>
		/// Sort the youngest particles to the front.
		/// </summary>
		YoungestInFront = 3,
	}
}
