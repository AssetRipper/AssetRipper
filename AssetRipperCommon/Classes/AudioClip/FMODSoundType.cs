namespace AssetRipper.Core.Classes.AudioClip
{
	public enum FMODSoundType
	{
		Unknown = 0x0,
		ACC = 0x1,
		AIFF = 0x2,
		ASF = 0x3,
		AT3 = 0x4,
		CDDA = 0x5,
		DLS = 0x6,
		FLAC = 0x7,
		FSB = 0x8,
		GCADPCM = 0x9,
		IT = 0xA,
		MIDI = 0xB,
		MOD = 0xC,
		MPEG = 0xD,
		OGGVORBIS = 0xE,
		PLAYLIST = 0xF,
		RAW = 0x10,
		S3M = 0x11,
		SF2 = 0x12,
		USER = 0x13,
		WAV = 0x14,
		XM = 0x15,
		XMA = 0x16,
		VAG = 0x17,
		AUDIOQUEUE = 0x18,
		XWMA = 0x19,
		BCWAV = 0x1A,
		AT9 = 0x1B,
		VORBIS = 0x1C,
		MEDIA_FOUNDATION = 0x1D,
		MAX = 0x1E,
		FORCEINT = 0x10000,
	}

	public static class FMODSoundTypeExtensions
	{
		public static string ToRawExtension(this FMODSoundType soundType) => soundType switch
		{
			FMODSoundType.ACC => "m4a",
			FMODSoundType.AIFF => "aif",
			FMODSoundType.IT => "it",
			FMODSoundType.MOD => "mod",
			FMODSoundType.MPEG => "mp3",
			FMODSoundType.OGGVORBIS => "ogg",
			FMODSoundType.S3M => "s3m",
			FMODSoundType.WAV => "wav",
			FMODSoundType.XM => "xm",
			FMODSoundType.XMA => "wav",
			FMODSoundType.VAG => "vag",
			FMODSoundType.AUDIOQUEUE => "fsb",
			_ => "audioClip",
		};
	}
}
