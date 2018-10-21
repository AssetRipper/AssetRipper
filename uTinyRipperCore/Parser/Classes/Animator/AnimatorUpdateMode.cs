namespace uTinyRipper.Classes.Animators
{
	/// <summary>
	/// The update mode of the Animator.
	/// </summary>
	public enum AnimatorUpdateMode
	{
		/// <summary>
		/// Normal update of the animator.
		/// </summary>
		Normal				= 0,
		/// <summary>
		/// Updates the animator during the physic loop in order to have the animation system synchronized with the physics engine.
		/// </summary>
		AnimatePhysics		= 1,
		/// <summary>
		/// Animator updates independently of Time.timeScale.
		/// </summary>
		UnscaledTime		= 2,
	}
}
