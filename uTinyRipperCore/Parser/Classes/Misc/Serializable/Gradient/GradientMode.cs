namespace uTinyRipper.Classes
{
	/// <summary>
	/// Select how gradients will be evaluated.
	/// </summary>
	public enum GradientMode
	{
		/// <summary>
		/// Find the 2 keys adjacent to the requested evaluation time, and linearly interpolate between them to obtain a blended color.
		/// </summary>
		Blend = 0,
		/// <summary>
		/// Return a fixed color, by finding the first key whose time value is greater than the requested evaluation time.
		/// </summary>
		Fixed = 1,
	}
}
