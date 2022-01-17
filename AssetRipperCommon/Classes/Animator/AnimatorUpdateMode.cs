namespace AssetRipper.Core.Classes.Animator
{
	/// <summary>
	/// The update mode of the Animator.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/Animation/ScriptBindings/Animator.bindings.cs"/>
	/// </summary>
	public enum AnimatorUpdateMode
	{
		/// <summary>
		/// Normal update of the animator.
		/// </summary>
		Normal = 0,
		/// <summary>
		/// Updates the animator during the physic loop in order to have the animation system synchronized with the physics engine.
		/// </summary>
		AnimatePhysics = 1,
		/// <summary>
		/// Animator updates independently of Time.timeScale.
		/// </summary>
		UnscaledTime = 2,
	}
}
