namespace uTinyRipper.Classes.Cameras
{
	public enum GateFitMode
	{
		/// <summary>
		/// Stretch the sensor gate to fit exactly into the resolution gate.
		/// </summary>
		None		= 0,
		/// <summary>
		/// Fit the resolution gate vertically within the sensor gate.
		/// </summary>
		Vertical	= 1,
		/// <summary>
		/// Fit the resolution gate horizontally within the sensor gate.
		/// </summary>
		Horizontal	= 2,
		/// <summary>
		/// Automatically selects a horizontal or vertical fit so that the sensor gate fits completely inside the resolution gate.
		/// </summary>
		Fill		= 3,
		/// <summary>
		/// Automatically selects a horizontal or vertical fit so that the render frame fits completely inside the resolution gate.
		/// </summary>
		Overscan	= 4,
	}
}
