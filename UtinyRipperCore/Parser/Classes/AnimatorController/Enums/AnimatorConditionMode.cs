namespace UtinyRipper.Classes.AnimatorControllers
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
		IfNot,
		/// <summary>
		/// The condition is true when parameter value is greater than the threshold.
		/// </summary>
		Greater,
		/// <summary>
		/// The condition is true when the parameter value is less than the threshold.
		/// </summary>
		Less,
		/// <summary>
		/// The condition is true when parameter value is equal to the threshold.
		/// </summary>
		Equals = 6,
		/// <summary>
		/// The condition is true when the parameter value is not equal to the threshold.
		/// </summary>
		NotEqual
	}
}
