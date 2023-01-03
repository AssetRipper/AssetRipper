using AssetRipper.SourceGenerated.Classes.ClassID_11;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AudioManagerExtensions
	{
		public static AudioSpeakerMode GetDefaultSpeakerMode(this IAudioManager audioManager)
		{
			return (AudioSpeakerMode)audioManager.Default_Speaker_Mode_C11;
		}
	}
}
