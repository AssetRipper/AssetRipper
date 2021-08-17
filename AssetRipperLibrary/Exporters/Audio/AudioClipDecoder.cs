using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Logging;
using Fmod5Sharp;
using Fmod5Sharp.FmodVorbis;
using System;
using System.Linq;

namespace AssetRipper.Library.Exporters.Audio
{
	public static class AudioClipDecoder
	{
		private static bool hasFailedToLoadLibPreviously;
		static AudioClipDecoder()
		{
			Pinvoke.DllLoader.PreloadDll("libogg");
			Pinvoke.DllLoader.PreloadDll("libvorbis");
		}
		public static byte[] GetDecodedAudioClipData(AudioClip audioClip)
		{
			if (hasFailedToLoadLibPreviously)
				return null;
			try
			{
				byte[] rawData = (byte[])audioClip.GetAudioData();
				FmodSoundBank fsbData = FsbLoader.LoadFsbFromByteArray(rawData);

				if (fsbData.Header.AudioType == FmodAudioType.VORBIS)
					return FmodVorbisRebuilder.RebuildOggFile(fsbData.Samples.Single());
				else
					return null;
			}
			catch (DllNotFoundException)
			{
				Logger.Error(LogCategory.Export, "Either LibVorbis or LibOgg is missing from your system, so Ogg audio clips cannot be exported. This message will not repeat.");
				hasFailedToLoadLibPreviously = true;
				return null;
			}
		}
	}
}
