namespace AssetRipper.Core.Classes.Animator
{
	/// <summary>
	/// Culling mode for the Animator.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/Animation/ScriptBindings/Animator.bindings.cs"/>
	/// </summary>
	public enum AnimatorCullingMode
	{
		/// <summary>
		/// Always animate the entire character. Object is animated even when offscreen.
		/// </summary>
		AlwaysAnimate = 0,
		/// <summary>
		/// Retarget, IK and write of Transforms are disabled when renderers are not visible.
		/// </summary>
		CullUpdateTransforms = 1,
		BasedOnRenderers = 1,
		/// <summary>
		/// Animation is completely disabled when renderers are not visible.
		/// </summary>
		CullCompletely = 2,
	}
}
