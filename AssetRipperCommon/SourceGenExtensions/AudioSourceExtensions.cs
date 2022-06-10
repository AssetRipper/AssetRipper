using AssetRipper.Core.Classes.AudioSource;
using AssetRipper.SourceGenerated.Classes.ClassID_82;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AudioSourceExtensions
	{
		public static AudioRolloffMode GetRolloffMode(this IAudioSource source)
		{
			return (AudioRolloffMode)source.RolloffMode_C82;
		}
	}
}
