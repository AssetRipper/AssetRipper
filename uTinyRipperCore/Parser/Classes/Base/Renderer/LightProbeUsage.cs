namespace uTinyRipper.Classes.Renderers
{
	/// <summary>
	/// Light probe interpolation type.
	/// </summary>
	public enum LightProbeUsage : byte
	{
		/// <summary>
		/// Light Probes are not used. The scene's ambient probe is provided to the shader.
		/// </summary>
		Off					= 0,
		/// <summary>
		/// Simple light probe interpolation is used.
		/// </summary>
		BlendProbes			= 1,
		/// <summary>
		/// Uses a 3D grid of interpolated light probes.
		/// </summary>
		UseProxyVolume		= 2,
		/// <summary>
		/// The light probe shader uniform values are extracted from the material property block set on the renderer.
		/// </summary>
		CustomProvided		= 4,
	}
}
