namespace uTinyRipper.Classes.Animators
{
	/// <summary>
	/// Culling mode for the Animator.
	/// </summary>
	public enum AnimatorCullingMode
	{
		/// <summary>
		/// Always animate the entire character. Object is animated even when offscreen.
		/// </summary>
		AlwaysAnimate			= 0,
		/// <summary>
		/// Retarget, IK and write of Transforms are disabled when renderers are not visible.
		/// </summary>
		CullUpdateTransforms	= 1,
		BasedOnRenderers		= 1,
		/// <summary>
		/// Animation is completely disabled when renderers are not visible.
		/// </summary>
		CullCompletely			= 2,
	}
}
