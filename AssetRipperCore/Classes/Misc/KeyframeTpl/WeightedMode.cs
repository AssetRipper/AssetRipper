namespace AssetRipper.Core.Classes.Misc.KeyframeTpl
{
	public enum WeightedMode
	{
		/// <summary>
		/// Exclude both inWeight or outWeight when calculating curve segments.
		/// </summary>
		None			= 0,
		/// <summary>
		/// Include inWeight when calculating the previous curve segment.
		/// </summary>
		In				= 1,
		/// <summary>
		/// Include outWeight when calculating the next curve segment.
		/// </summary>
		Out				= 2,
		/// <summary>
		/// Include inWeight and outWeight when calculating curve segments.
		/// </summary>
		Both			= 3,
	}
}
