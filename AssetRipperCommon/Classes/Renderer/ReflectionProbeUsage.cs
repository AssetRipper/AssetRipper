namespace AssetRipper.Core.Classes.Renderer
{
	/// <summary>
	/// Reflection Probe usage.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum ReflectionProbeUsage
	{
		/// <summary>
		/// Reflection probes are disabled, skybox will be used for reflection.
		/// </summary>
		Off = 0,
		/// <summary>
		/// Reflection probes are enabled. Blending occurs only between probes, useful in indoor environments.
		/// The renderer will use default reflection if there are no reflection probes nearby,
		/// but no blending between default reflection and probe will occur.
		/// </summary>
		BlendProbes = 1,
		/// <summary>
		/// Reflection probes are enabled. Blending occurs between probes or probes and default reflection, useful for outdoor environments.
		/// </summary>
		BlendProbesAndSkybox = 2,
		/// <summary>
		/// Reflection probes are enabled, but no blending will occur between probes when there are two overlapping volumes.
		/// </summary>
		Simple = 3,
	}
}
