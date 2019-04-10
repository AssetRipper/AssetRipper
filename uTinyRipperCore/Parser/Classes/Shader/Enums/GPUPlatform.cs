namespace uTinyRipper.Classes.Shaders
{
	/// <summary>
	/// Graphic API
	/// </summary>
	public enum GPUPlatform
	{
		/// <summary>
		/// For inner use only
		/// </summary>
		unknown			= -1,

		openGL			= 0,
		d3d9			= 1,
		xbox360			= 2,
		ps3				= 3,
		d3d11			= 4,
		gles			= 5,
		glesdesktop		= 6,
		flash			= 7,
		d3d11_9x		= 8,
		gles3			= 9,
		psp2			= 10,
		ps4				= 11,
		xboxone			= 12,
		psm				= 13,
		metal			= 14,
		glcore			= 15,
		n3ds			= 16,
		wiiu			= 17,
		vulkan			= 18,
		@switch			= 19,
		xboxone_d3d12	= 20,
	}
}
