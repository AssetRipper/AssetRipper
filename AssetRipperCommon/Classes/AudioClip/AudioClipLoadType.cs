namespace AssetRipper.Core.Classes.AudioClip
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/Audio/Public/ScriptBindings/Audio.bindings.cs"/>
	/// </summary>
	public enum AudioClipLoadType
	{
		DecompressOnLoad = 0,
		CompressedInMemory = 1,
		/// <summary>
		/// StreamFromDisc previously
		/// </summary>
		Streaming = 2,
	}
}
