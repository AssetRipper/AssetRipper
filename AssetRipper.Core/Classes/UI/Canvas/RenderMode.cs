namespace AssetRipper.Core.Classes.UI.Canvas
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UI/ScriptBindings/UICanvas.bindings.cs"/>
	/// </summary>
	public enum RenderMode
	{
		/// <summary>
		/// Render at the end of the scene using a 2D Canvas.
		/// </summary>
		ScreenSpaceOverlay = 0,
		/// <summary>
		/// Render using the Camera configured on the Canvas.
		/// </summary>
		ScreenSpaceCamera = 1,
		/// <summary>
		/// Render using any Camera in the scene that can render the layer.
		/// </summary>
		WorldSpace = 2
	}
}
