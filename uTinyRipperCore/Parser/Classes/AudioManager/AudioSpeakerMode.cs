namespace uTinyRipper.Classes.AudioManagers
{
	public enum AudioSpeakerMode
	{
		/// <summary>
		/// Channel count is unaffected.
		/// </summary>
		Raw				= 0,
		/// <summary>
		/// Channel count is set to 1. The speakers are monaural.
		/// </summary>
		Mono			= 1,
		/// <summary>
		/// Channel count is set to 2. The speakers are stereo. This is the editor default.
		/// </summary>
		Stereo			= 2,
		/// <summary>
		/// Channel count is set to 4. 4 speaker setup. This includes front left, front right, rear left, rear right.
		/// </summary>
		Quad			= 3,
		/// <summary>
		/// Channel count is set to 5. 5 speaker setup. This includes front left, front right, center, rear left, rear right.
		/// </summary>
		Surround		= 4,
		/// <summary>
		/// Channel count is set to 6. 5.1 speaker setup. This includes front left, front right, center, rear left, rear right and a subwoofer.
		/// </summary>
		Mode5point1		= 5,
		/// <summary>
		/// Channel count is set to 8. 7.1 speaker setup. This includes front left, front right, center, rear left, rear right, side left, side right and a subwoofer.
		/// </summary>
		Mode7point1		= 6,
		/// <summary>
		/// Channel count is set to 2. Stereo output, but data is encoded in a way that is picked up by a Prologic/Prologic2 decoder and split into a 5.1 speaker setup.
		/// </summary>
		Prologic		= 7,
	}
}
