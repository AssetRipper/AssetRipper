namespace AssetRipper.Core.Classes.ParticleSystem.CollisionModule
{
	/// <summary>
	/// Quality of world collisions. Medium and low quality are approximate and may leak particles.
	/// </summary>
	public enum ParticleSystemCollisionQuality
	{
		/// <summary>
		/// The most accurate world collisions.
		/// </summary>
		High = 0,
		/// <summary>
		/// Approximate world collisions.
		/// </summary>
		Medium = 1,
		/// <summary>
		/// Fastest and most approximate world collisions.
		/// </summary>
		Low = 2,
	}
}
