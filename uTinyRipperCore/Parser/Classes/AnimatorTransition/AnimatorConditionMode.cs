namespace uTinyRipper.Classes.AnimatorTransitions
{
	/// <summary>
	/// The mode of the condition.
	/// </summary>
	public enum AnimatorConditionMode
	{
		/// <summary>
		/// The condition is true when the parameter value is true.
		/// </summary>
		If = 1,
		/// <summary>
		/// The condition is true when the parameter value is false.
		/// </summary>
		IfNot = 2,
		/// <summary>
		/// The condition is true when parameter value is greater than the threshold.
		/// </summary>
		Greater = 3,
		/// <summary>
		/// The condition is true when the parameter value is less than the threshold.
		/// </summary>
		Less = 4,
		/// <summary>
		/// The condition is true when the source state has stepped over the exit time value.
		/// </summary>
		ExitTime = 5,
		/// <summary>
		/// The condition is true when parameter value is equal to the threshold.
		/// </summary>
		Equals = 6,
		/// <summary>
		/// The condition is true when the parameter value is not equal to the threshold.
		/// </summary>
		NotEqual = 7,
	}
}
