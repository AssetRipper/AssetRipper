namespace AssetRipper.Core.Classes.ParticleSystemRenderer
{
	/// <summary>
	/// The rendering mode for particle systems
	/// </summary>
	public enum ParticleSystemRenderMode
	{
		/// <summary>
		/// Render particles as billboards facing the active camera.
		/// </summary>
		Billboard = 0,
		/// <summary>
		/// Stretch particles in the direction of motion.
		/// </summary>
		Stretch = 1,
		/// <summary>
		/// Render particles as billboards always facing up along the y-Axis.
		/// </summary>
		HorizontalBillboard = 2,
		/// <summary>
		/// Render particles as billboards always facing the player, but not pitching along the x-Axis.
		/// </summary>
		VerticalBillboard = 3,
		/// <summary>
		/// Render particles as meshes.
		/// </summary>
		Mesh = 4,
		/// <summary>
		/// Do not render particles.
		/// </summary>
		None = 5,
	}
}
