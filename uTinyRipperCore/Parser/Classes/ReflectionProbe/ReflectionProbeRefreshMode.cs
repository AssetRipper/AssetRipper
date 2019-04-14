namespace uTinyRipper.Classes.ReflectionProbes
{
	public enum ReflectionProbeRefreshMode
	{
		/// <summary>
		/// Causes the probe to update only on the first frame it becomes visible. The probe will no longer update automatically, however you may subsequently use RenderProbe to refresh the probe
		/// See Also: ReflectionProbe.RenderProbe.
		/// </summary>
		OnAwake			= 0,
		/// <summary>
		/// Causes Unity to update the probe's cubemap every frame.
		/// Note that updating a probe is very costly. Setting this option on too many probes could have a significant negative effect on frame rate. Use time-slicing to help improve performance.
		/// See Also: ReflectionProbeTimeSlicingMode.
		/// </summary>
		EveryFrame		= 1,
		/// <summary>
		/// Sets the probe to never be automatically updated by Unity while your game is running. Use this to completely control the probe refresh behavior by script.
		/// See Also: ReflectionProbe.RenderProbe.
		/// </summary>
		ViaScripting	= 2,
	}
}
