namespace uTinyRipper.Classes.Renderers
{
	/// <summary>
	/// Determines if the object receives Global Illumination from its surroundings through either Lightmaps or LightProbes.
	/// Forced to LightProbes if Contribute GI is turned off
	/// </summary>
	public enum ReceiveGI
	{
		/// <summary>
		/// Makes the GameObject use lightmaps to receive Global Illumination
		/// </summary>
		Lightmaps	= 1,
		/// <summary>
		/// The object will have the option to use Light Probes to receive Global Illumination. See LightProbeUsage
		/// </summary>
		LightProbes = 2,
	}
}
