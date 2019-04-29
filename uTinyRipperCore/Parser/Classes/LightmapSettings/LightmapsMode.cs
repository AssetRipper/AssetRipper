using System;

namespace uTinyRipper.Classes.LightmapSettingss
{
	[Flags]
	public enum LightmapsMode
	{
		/// <summary>
		/// Light intensity (no directional information), encoded as 1 lightmap.
		/// </summary>
		NonDirectional		= 0,
		Single				= 0,
		/// <summary>
		/// Directional information for direct light is combined with directional information for indirect light, encoded as 2 lightmaps.
		/// </summary>
		CombinedDirectional	= 1,
		Dual				= 1,
		/// <summary>
		/// Directional information for direct light is stored separately from directional information for indirect light, encoded as 4 lightmaps.
		/// </summary>
		SeparateDirectional	= 2,
		Directional			= 2
	}
}
