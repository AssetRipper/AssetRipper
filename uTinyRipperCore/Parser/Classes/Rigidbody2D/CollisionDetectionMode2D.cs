namespace uTinyRipper.Classes.Rigidbody2Ds
{
	public enum CollisionDetectionMode2D
	{
		/// <summary>
		/// Obsolete. Use Discrete instead.
		/// </summary>
		None = 0,
		/// <summary>
		/// Bodies move but may cause colliders to pass through other colliders at higher speeds
		/// but is much faster to calculate than continuous mode.
		/// </summary>
		Discrete = 0,
		/// <summary>
		/// Provides the most accurate collision detection to prevent colliders passing through other colliders at higher speeds
		/// but is much more expensive to calculate.
		/// </summary>
		Continuous = 1
	}
}
