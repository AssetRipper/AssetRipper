namespace AssetRipper.Core.Classes.LightmapSettings
{
	/// <summary>
	/// Which path tracer sampling scheme is used.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/GI/LightingSettings.bindings.cs"/>
	/// </summary>
	public enum Sampling
	{
		/// <summary>
		/// Convergence testing is automatic, stops when lightmap has converged.
		/// </summary>
		Auto = 0,
		/// <summary>
		/// No convergence testing, always uses the given number of samples.
		/// </summary>
		Fixed = 1,
	}
}
