namespace AssetRipper.Core.Classes.Mesh
{
	/// <summary>
	/// Compressing meshes saves space in the built game, but more compression introduces more artifacts in vertex data.
	/// </summary>
	public enum MeshCompression : byte
	{
		/// <summary>
		/// No mesh compression (default).
		/// </summary>
		Off = 0,
		/// <summary>
		/// Low amount of mesh compression.
		/// </summary>
		Low = 1,
		/// <summary>
		/// Medium amount of mesh compression.
		/// </summary>
		Med = 2,
		/// <summary>
		/// High amount of mesh compression.
		/// </summary>
		High = 3,
		Count,
	}
}
