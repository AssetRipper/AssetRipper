namespace uTinyRipper.Classes.LODGroups
{
	/// <summary>
	/// The LOD fade modes. Modes other than LODFadeMode.None will result in Unity calculating a blend factor
	/// for blending/interpolating between two neighbouring LODs and pass it to your shader.
	/// </summary>
	public enum LODFadeMode
	{
		/// <summary>
		/// Indicates the LOD fading is turned off.
		/// </summary>
		None		= 0,
		/// <summary>
		/// Perform cross-fade style blending between the current LOD and the next LOD if the distance to camera falls
		/// in the range specified by the LOD.fadeTransitionWidth of each LOD.
		/// </summary>
		CrossFade	= 1,
		/// <summary>
		/// By specifying this mode, your LODGroup will perform a SpeedTree-style LOD fading scheme:
		/// * For all the mesh LODs other than the last (most crude) mesh LOD, the fade factor is calculated as the percentage of the object's current screen height, compared to the whole range of the LOD. It is 1, if the camera is right at the position where the previous LOD switches out and 0, if the next LOD is just about to switch in.
		/// * For the last mesh LOD and the billboard LOD, the cross-fade mode is used.
		/// </summary>
		SpeedTree	= 2,
	}
}
