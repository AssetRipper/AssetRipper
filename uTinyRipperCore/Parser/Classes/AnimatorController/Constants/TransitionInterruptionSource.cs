namespace uTinyRipper.Classes.AnimatorControllers
{
	/// <summary>
	/// Which AnimatorState transitions can interrupt the Transition.
	/// </summary>
	public enum TransitionInterruptionSource
	{
		/// <summary>
		/// The Transition cannot be interrupted. Formely know as Atomic.
		/// </summary>
		None = 0,
		/// <summary>
		/// The Transition can be interrupted by transitions in the source AnimatorState.
		/// </summary>
		Source = 1,
		/// <summary>
		/// The Transition can be interrupted by transitions in the destination AnimatorState.
		/// </summary>
		Destination = 2,
		/// <summary>
		/// The Transition can be interrupted by transitions in the source or the destination AnimatorState.
		/// </summary>
		SourceThenDestination = 3,
		/// <summary>
		/// The Transition can be interrupted by transitions in the source or the destination AnimatorState.
		/// </summary>
		DestinationThenSource = 4
	}
}
