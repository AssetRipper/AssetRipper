﻿namespace AssetRipper.Core.Classes.QualitySettings
{
	/// <summary>
	/// The rendering mode of Shadowmask.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum ShadowmaskMode
	{
		/// <summary>
		/// Static shadow casters won't be rendered into realtime shadow maps.
		/// ll shadows from static casters are handled via Shadowmasks and occlusion from Light Probes.
		/// </summary>
		Shadowmask = 2,
		/// <summary>
		/// Static shadow casters will be rendered into realtime shadow maps.
		/// Shadowmasks and occlusion from Light Probes will only be used past the realtime shadow distance.
		/// </summary>
		DistanceShadowmask = 1,
	}
}
