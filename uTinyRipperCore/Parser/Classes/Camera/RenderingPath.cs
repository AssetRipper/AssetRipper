namespace uTinyRipper.Classes.Cameras
{
	/// <summary>
	/// Rendering path of a Camera.
	/// </summary>
	public enum RenderingPath
	{
		UsePlayerSettings	= -1,
		VertexLit			= 0,
		Forward				= 1,
		DeferredLighting	= 2,
		DeferredShading		= 3
	}
}
