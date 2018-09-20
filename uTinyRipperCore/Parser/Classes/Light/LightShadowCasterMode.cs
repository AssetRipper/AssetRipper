namespace uTinyRipper.Classes.Lights
{
	/// <summary>
	/// Allows mixed lights to control shadow caster culling when Shadowmasks are present.
	/// </summary>
	public enum LightShadowCasterMode
	{
		/// <summary>
		/// Use the global Shadowmask Mode from the quality settings.
		/// </summary>
		Default				= 0,
		/// <summary>
		/// Render only non-lightmapped objects into the shadow map. This corresponds with the Shadowmask mode.
		/// </summary>
		NonLightmappedOnly	= 1,
		/// <summary>
		/// Render all shadow casters into the shadow map. This corresponds with the distance Shadowmask mode.
		/// </summary>
		Everything			= 2,
	}
}
