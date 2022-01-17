namespace AssetRipper.Core.Classes.LightmapSettings
{
	/// <summary>
	/// Which baking backend is used.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/GI/LightingSettings.bindings.cs"/>
	/// </summary>
	public enum Lightmapper
	{
		Radiosity = 0,
		/// <summary>
		/// Lightmaps are baked by Enlighten
		/// </summary>
		Enlighten = 0,
		PathTracer = 1,
		/// <summary>
		/// Lightmaps are baked by the CPU Progressive lightmapper (Wintermute + OpenRL based).
		/// </summary>
		ProgressiveCPU = 1,
		/// <summary>
		/// Lightmaps are baked by the GPU Progressive lightmapper (RadeonRays + OpenCL based).
		/// </summary>
		ProgressiveGPU = 2,
	}
}
