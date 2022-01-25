namespace AssetRipper.Core.Classes.TerrainData
{
	/// <summary>
	/// Render mode for detail prototypes.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/Terrain/Public/TerrainData.bindings.cs"/>
	/// </summary>
	public enum DetailRenderMode
	{
		/// <summary>
		/// The detail prototype will be rendered as billboards that are always facing the camera.
		/// </summary>
		GrassBillboard = 0,
		/// <summary>
		/// Will show the prototype using diffuse shading.
		/// </summary>
		VertexLit = 1,
		/// <summary>
		/// The detail prototype will use the grass shader.
		/// </summary>
		Grass = 2,
	}
}
