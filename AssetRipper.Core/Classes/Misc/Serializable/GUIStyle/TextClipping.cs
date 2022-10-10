namespace AssetRipper.Core.Classes.Misc.Serializable.GUIStyle
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/IMGUI/GUIStyle.cs"/>
	/// </summary>
	public enum TextClipping
	{
		/// <summary>
		/// Text flows freely outside the element.
		/// </summary>
		Overflow = 0,
		/// <summary>
		/// Text gets clipped to be inside the element.
		/// </summary>
		Clip = 1,
		/// <summary>
		/// Text gets truncated with dots to show it is too long.
		/// </summary>
		Truncate = 2,
	}
}
