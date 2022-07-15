using AssetRipper.Core.Classes.AudioReverbFilter;
using AssetRipper.SourceGenerated.Classes.ClassID_164;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AudioReverbFilterExtensions
	{
		public static AudioReverbPreset GetReverbPreset(this IAudioReverbFilter audioReverbFilter)
		{
			return (AudioReverbPreset)audioReverbFilter.ReverbPreset_C164;
		}
	}
}
