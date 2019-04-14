namespace uTinyRipper.Classes.ReflectionProbes
{
	/// <summary>
	/// Reflection probe's update mode.
	/// </summary>
	public enum ReflectionProbeMode
	{
		/// <summary>
		/// Reflection probe is baked in the Editor.
		/// </summary>
		Baked		= 0,
		/// <summary>
		/// Reflection probe is updating in realtime.
		/// </summary>
		Realtime	= 1,
		/// <summary>
		/// Reflection probe uses a custom texture specified by the user.
		/// </summary>
		Custom		= 2,
	}
}
