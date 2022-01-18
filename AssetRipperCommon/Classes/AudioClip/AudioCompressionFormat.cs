namespace AssetRipper.Core.Classes.AudioClip
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/Audio/Public/ScriptBindings/Audio.bindings.cs"/>
	/// </summary>
	public enum AudioCompressionFormat
	{
		PCM = 0,
		Vorbis = 1,
		ADPCM = 2,
		MP3 = 3,
		VAG = 4,
		HEVAG = 5,
		XMA = 6,
		AAC = 7,
		GCADPCM = 8,
		ATRAC9 = 9,
	}

	public static class AudioCompressionFormatExtentions
	{
		public static string ToRawExtension(this AudioCompressionFormat compressionFormat)
		{
			switch (compressionFormat)
			{
				case AudioCompressionFormat.PCM:
				case AudioCompressionFormat.Vorbis:
				case AudioCompressionFormat.ADPCM:
				case AudioCompressionFormat.MP3:
				case AudioCompressionFormat.GCADPCM:
					return "fsb";
				case AudioCompressionFormat.VAG:
				case AudioCompressionFormat.HEVAG:
					return "vag";
				case AudioCompressionFormat.XMA:
					return "wav";
				case AudioCompressionFormat.AAC:
					return "m4a";
				case AudioCompressionFormat.ATRAC9:
					return "at9";
				default:
					return "audioClip";
			}
		}
	}
}
