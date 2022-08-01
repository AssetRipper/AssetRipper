namespace AssetRipper.Library.Configuration
{
	public enum MeshCoordinateSpace
	{
		/// <summary>
		/// Coordinates for non-yaml export will remain in the Unity Coordinate space
		/// </summary>
		Unity,
		/// <summary>
		/// Coordinates for non-yaml export will be converted to the Left-Handed Coordinate space
		/// </summary>
		Left,
		/// <summary>
		/// Coordinates for non-yaml export will be converted to the Right-Handed Coordinate space
		/// </summary>
		Right
	}
}
