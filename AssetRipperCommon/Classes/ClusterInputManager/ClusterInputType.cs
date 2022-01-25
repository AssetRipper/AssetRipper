namespace AssetRipper.Core.Classes.ClusterInputManager
{
	/// <summary>
	/// Values to determine the type of input value to be expect from one entry of ClusterInput.
	/// </summary>
	public enum ClusterInputType
	{
		/// <summary>
		/// Device that return a binary result of pressed or not pressed.
		/// </summary>
		Button = 0,
		/// <summary>
		/// Device is an analog axis that provides continuous value represented by a float.
		/// </summary>
		Axis = 1,
		/// <summary>
		/// Device that provide position and orientation values.
		/// </summary>
		Tracker = 2,
		/// <summary>
		/// A user customized input.
		/// </summary>
		CustomProvidedInput = 3,
	}
}
