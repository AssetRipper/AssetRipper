namespace AssetRipper.Core.Classes.AudioClip
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Audio/AudioType.cs"/>
	/// </summary>
	public enum AudioType
	{
		/// <summary>
		/// 3rd party / unknown plugin format.
		/// </summary>
		UNKNOWN = 0,
		/// <summary>
		/// Depreciated at some point
		/// </summary>
		ACC = 1,
		/// <summary>
		/// aiff
		/// </summary>
		AIFF = 2,
		/// <summary>
		/// Microsoft Advanced Systems Format (ie WMA/ASF/WMV).
		/// </summary>
		ASF = 3,
		/// <summary>
		/// Sony ATRAC 3 format
		/// </summary>
		AT3 = 4,
		/// <summary>
		/// Digital CD audio.
		/// </summary>
		CDDA = 5,
		/// <summary>
		/// Sound font / downloadable sound bank.
		/// </summary>
		DLS = 6,
		/// <summary>
		/// FLAC lossless codec.
		/// </summary>
		FLAC = 7,
		/// <summary>
		/// FMOD Sample Bank.
		/// </summary>
		FSB = 8,
		/// <summary>
		/// GameCube ADPCM
		/// </summary>
		GCADPCM = 9,
		/// <summary>
		/// Impulse Tracker
		/// </summary>
		IT = 10,
		/// <summary>
		/// MIDI.
		/// </summary>
		MIDI = 11,
		/// <summary>
		/// Protracker / Fasttracker MOD.
		/// </summary>
		MOD = 12,
		/// <summary>
		/// MP2/MP3 MPEG.
		/// </summary>
		MPEG = 13,
		/// <summary>
		/// ogg vorbis
		/// </summary>
		OGGVORBIS = 14,
		/// <summary>
		/// Information only from ASX/PLS/M3U/WAX playlists
		/// </summary>
		PLAYLIST = 15,
		/// <summary>
		/// Raw PCM data.
		/// </summary>
		RAW = 16,
		/// <summary>
		/// ScreamTracker 3.
		/// </summary>
		S3M = 17,
		/// <summary>
		/// Sound font 2 format.
		/// </summary>
		SF2 = 18,
		/// <summary>
		/// User created sound.
		/// </summary>
		USER = 19,
		/// <summary>
		/// Microsoft WAV.
		/// </summary>
		WAV = 20,
		/// <summary>
		/// FastTracker 2 XM.
		/// </summary>
		XM = 21,
		/// <summary>
		/// XboxOne XMA(2)
		/// </summary>
		XMA = 22,
		/// <summary>
		/// PlayStation 2 / PlayStation Portable adpcm VAG format.
		/// </summary>
		VAG = 23,
		/// <summary>
		/// iPhone hardware decoder, supports AAC, ALAC and MP3. Extracodecdata is a pointer to an FMOD_AUDIOQUEUE_EXTRACODECDATA structure.
		/// </summary>
		AUDIOQUEUE = 24,
		/// <summary>
		/// Xbox360 XWMA
		/// </summary>
		XWMA = 25,
		/// <summary>
		/// 3DS BCWAV container format for DSP ADPCM and PCM
		/// </summary>
		BCWAV = 26,
		/// <summary>
		/// NGP ATRAC 9 format
		/// </summary>
		AT9 = 27,
		PCM = 28,
		ADPCM = 29,
	}
}
