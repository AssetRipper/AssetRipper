namespace uTinyRipper.Classes.ReflectionProbes
{
	/// <summary>
	/// When a probe's ReflectionProbe.refreshMode is set to ReflectionProbeRefreshMode.EveryFrame, this enum specify whether or not Unity should update the probe's cubemap over several frames or update the whole cubemap in one frame.
	/// Updating a probe's cubemap is a costly operation. Unity needs to render the entire Scene for each face of the cubemap, as well as perform special blurring in order to get glossy reflections. The impact on frame rate can be significant. Time-slicing helps maintaning a more constant frame rate during these updates by performing the rendering over several frames.
	/// </summary>
	public enum ReflectionProbeTimeSlicingMode
	{
		/// <summary>
		/// Instructs Unity to use time-slicing by first rendering all faces at once, then spreading the remaining work over the next 8 frames. Using this option, updating the probe will take 9 frames.
		/// </summary>
		AllFacesAtOnce		= 0,
		/// <summary>
		/// Instructs Unity to spread the rendering of each face over several frames. Using this option, updating the cubemap will take 14 frames. This option greatly reduces the impact on frame rate, however it may produce incorrect results, especially in Scenes where lighting conditions change over these 14 frames.
		/// </summary>
		IndividualFaces		= 1,
		/// <summary>
		/// Unity will render the probe entirely in one frame.
		/// </summary>
		NoTimeSlicing		= 2,
	}
}
