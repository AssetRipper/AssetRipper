using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

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
