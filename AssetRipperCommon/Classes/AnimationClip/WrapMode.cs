namespace AssetRipper.Core.Classes.AnimationClip
{
	/// <summary>
	/// Determines how time is treated outside of the keyframed range of an AnimationClip or AnimationCurve.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Animation/AnimationCurve.bindings.cs"/>
	/// </summary>
	public enum WrapMode
	{
		/// <summary>
		/// Reads the default repeat mode set higher up.
		/// </summary>
		Default = 0,
		Clamp = 1,
		/// <summary>
		/// When time reaches the end of the animation clip, the clip will automatically stop playing and time will be reset to beginning of the clip.
		/// </summary>
		Once = 1,
		/// <summary>
		/// When time reaches the end of the animation clip, time will continue at the beginning.
		/// </summary>
		Loop = 2,
		/// <summary>
		/// When time reaches the end of the animation clip, time will ping pong back between beginning and end.
		/// </summary>
		PingPong = 4,
		/// <summary>
		/// Plays back the animation. When it reaches the end, it will keep playing the last frame and never stop playing.
		/// </summary>
		ClampForever = 8,
	}
}
