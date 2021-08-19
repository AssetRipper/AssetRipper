using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Logging;
using Fmod5Sharp;
using Fmod5Sharp.FmodVorbis;
using OggVorbisSharp;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace AssetRipper.Library.Exporters.Audio
{
	public static class AudioClipDecoder
	{
		public static bool LibrariesLoaded { get; private set; }
		static AudioClipDecoder()
		{
			LibrariesLoaded = IsVorbisLoaded() && IsOggLoaded();
			if (!LibrariesLoaded)
				Logger.Error(LogCategory.Export, "Either LibVorbis or LibOgg is missing from your system, so Ogg audio clips cannot be exported. This message will not repeat.");
		}
		public static byte[] GetDecodedAudioClipData(AudioClip audioClip)
		{
			if (!LibrariesLoaded)
				return null;

			byte[] rawData = (byte[])audioClip.GetAudioData();
			FmodSoundBank fsbData = FsbLoader.LoadFsbFromByteArray(rawData);

			if (fsbData.Header.AudioType == FmodAudioType.VORBIS)
				return FmodVorbisRebuilder.RebuildOggFile(fsbData.Samples.Single());
			else
				return null;
		}
		private unsafe static bool IsVorbisLoaded()
		{
			try { OggVorbisSharp.Vorbis.vorbis_version_string(); }
			catch (DllNotFoundException)
			{
				return false;
			}
			return true;
		}
		private unsafe static bool IsOggLoaded()
		{
			bool result = true;
			ogg_stream_state* streamPtr = (ogg_stream_state*)Marshal.AllocHGlobal(sizeof(OggVorbisSharp.ogg_stream_state));
			*streamPtr = new ogg_stream_state();
			try
			{
				OggVorbisSharp.Ogg.ogg_stream_init(streamPtr, 1);
			}
			catch (DllNotFoundException)
			{
				result = false;
			}
			finally
			{
				Marshal.FreeHGlobal((IntPtr)streamPtr);
			}
			return result;
		}
	}
}
