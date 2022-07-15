namespace AssetRipper.Core.Classes.GraphicsSettings
{
	/// <summary>
	/// Graphics Tier.
	/// </summary>
	public enum GraphicsTier
	{
		/// <summary>
		/// The first graphics tier (Low) - corresponds to shader define UNITY_HARDWARE_TIER1.
		/// </summary>
		Tier1 = 0,
		/// <summary>
		/// The second graphics tier (Medium) - corresponds to shader define UNITY_HARDWARE_TIER2.
		/// </summary>
		Tier2 = 1,
		/// <summary>
		/// The third graphics tier (High) - corresponds to shader define UNITY_HARDWARE_TIER3.
		/// </summary>
		Tier3 = 2,
	}
}
