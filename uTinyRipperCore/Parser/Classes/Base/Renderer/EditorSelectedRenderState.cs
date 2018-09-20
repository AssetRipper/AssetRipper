namespace uTinyRipper.Classes.Renderers
{
	/// <summary>
	/// The editor selected render mode for Scene View selection.
	/// </summary>
	public enum EditorSelectedRenderState
	{
		/// <summary>
		/// The Renderer has no selection highlight or wireframe in the Editor.
		/// </summary>
		Hidden		= 0,
		/// <summary>
		/// The Renderer has wireframe but not selection highlight in the Editor.
		/// </summary>
		Wireframe	= 1,
		/// <summary>
		/// The Renderer has selection highlight but no wireframe in the Editor.
		/// </summary>
		Highlight	= 2
	}
}
