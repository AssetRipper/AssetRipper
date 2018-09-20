namespace uTinyRipper.Classes.RenderSettingss
{
	public enum AmbientMode
	{
		/// <summary>
		/// Skybox-based or custom ambient lighting.
		/// </summary>
		Skybox			= 0,
		/// <summary>
		/// Trilight ambient lighting.
		/// </summary>
		Trilight		= 1,
		/// <summary>
		/// Flat ambient lighting.
		/// </summary>
		Flat			= 3,
		/// <summary>
		/// Ambient lighting is defined by a custom cubemap.
		/// </summary>
		Custom			= 4,
	}
}
