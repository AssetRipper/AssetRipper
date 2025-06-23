using AssetRipper.SourceGenerated.NativeEnums.Fmod;

namespace AssetRipper.SourceGenerated.Extensions;

public static class FmodSoundTypeExtensions
{
	public static string ToRawExtension(this FmodSoundType soundType) => soundType switch
	{
		FmodSoundType.Acc => "m4a",
		FmodSoundType.Aiff => "aif",
		FmodSoundType.It => "it",
		FmodSoundType.Mod => "mod",
		FmodSoundType.Mpeg => "mp3",
		FmodSoundType.Oggvorbis => "ogg",
		FmodSoundType.S3m => "s3m",
		FmodSoundType.Wav => "wav",
		FmodSoundType.Xm => "xm",
		FmodSoundType.Xma => "wav",
		FmodSoundType.Vag => "vag",
		FmodSoundType.Audioqueue => "fsb",
		_ => "audioClip",
	};
}
