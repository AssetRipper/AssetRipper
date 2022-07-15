namespace AssetRipper.Core.Classes.LightmapSettings
{
	/// <summary>
	/// Which path tracer denoiser is used.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/GI/LightingSettings.bindings.cs"/>
	/// </summary>
	public enum DenoiserType
	{
		/// <summary>
		/// No denoiser
		/// </summary>
		None = 0,
		/// <summary>
		/// The NVIDIA Optix AI denoiser is applied.
		/// </summary>
		Optix = 1,
		/// <summary>
		/// The Intel Open Image AI denoiser is applied.
		/// </summary>
		OpenImage = 2,
		/// <summary>
		/// The AMD Radeon Pro Image Processing denoiser is applied.
		/// </summary>
		RadeonPro = 3,
	}
}
