using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Logging;
using Fmod5Sharp;
using OggVorbisSharp;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AssetRipper.Library.Exporters.Audio
{
	public static class AudioClipDecoder
	{
		/// <summary>
		/// Are Ogg and Vorbis loaded?
		/// </summary>
		public static bool LibrariesLoaded { get; private set; }
		/// <summary>
		/// Size of the magic number currently
		/// </summary>
		private const int MinimumFsbSize = 4;

		static AudioClipDecoder()
		{
			NativeLibrary.SetDllImportResolver(typeof(Ogg).Assembly, DllImportResolver);
			LibrariesLoaded = IsVorbisLoaded() & IsOggLoaded();
			if (!LibrariesLoaded)
				Logger.Error(LogCategory.Export, "Either LibVorbis or LibOgg is missing from your system, so Ogg audio clips cannot be exported. This message will not repeat.");
		}

		public static bool CanDecode(IAudioClip audioClip)
		{
			byte[] rawData = (byte[])audioClip?.GetAudioData();
			if (!IsDataUsable(rawData))
				return false;

			FmodAudioType audioType = GetAudioType(rawData);
			if (audioType == FmodAudioType.VORBIS && !LibrariesLoaded)
			{
				return false;
			}
			else if (FmodAudioTypeExtensions.IsSupported(audioType))
			{
				return true;
			}
			else
			{
				Logger.Info(LogCategory.Export, $"Can't decode audio clip '{audioClip.Name}' with default decoder because it's '{audioType}' encoded.");
				return false;
			}
		}

		public static bool TryGetDecodedAudioClipData(IAudioClip audioClip, out byte[] decodedData, out string fileExtension)
		{
			return TryGetDecodedAudioClipData((byte[])audioClip?.GetAudioData(), out decodedData, out fileExtension);
		}
		public static bool TryGetDecodedAudioClipData(byte[] rawData, out byte[] decodedData, out string fileExtension)
		{
			decodedData = null;
			fileExtension = null;

			if (!IsDataUsable(rawData))
				return false;

			FmodSoundBank fsbData = FsbLoader.LoadFsbFromByteArray(rawData);

			var audioType = fsbData.Header.AudioType;
			try
			{
				if (audioType == FmodAudioType.VORBIS && !LibrariesLoaded)
					return false;
				else if (audioType.IsSupported() && fsbData.Samples.Single().RebuildAsStandardFileFormat(out decodedData, out fileExtension))
					return true;
				else
					return false;
			}
			catch (Exception ex)
			{
				Logger.Error(LogCategory.Export, $"Failed to convert audio ({Enum.GetName(audioType)})", ex);
				return false;
			}
		}

		/// <summary>
		/// Decodes WAV data from an AudioClip
		/// </summary>
		/// <param name="audioClip">The audio clip to extract the data from</param>
		/// <param name="decodedData">The decoded data in the wav audio format</param>
		/// <returns>True if the audio could be exported in the wav format</returns>
		public static bool TryGetDecodedWavData(IAudioClip audioClip, out byte[] decodedData)
		{
			return TryGetDecodedWavData(audioClip?.GetAudioData(), out decodedData);
		}
		/// <summary>
		/// Decodes WAV data from FSB data
		/// </summary>
		/// <param name="fsbData">The data from an FSB file</param>
		/// <param name="decodedData">The decoded data in the wav audio format</param>
		/// <returns>True if the audio could be exported in the wav format</returns>
		public static bool TryGetDecodedWavData(byte[] fsbData, out byte[] decodedData)
		{
			if (TryGetDecodedAudioClipData(fsbData, out decodedData, out string fileExtension))
			{
				if (fileExtension == "ogg")
				{
					decodedData = AudioConverter.OggToWav(decodedData);
					return true;
				}
				else
					return fileExtension == "wav";
			}
			else
			{
				decodedData = null;
				return false;
			}
		}

		public static FmodAudioType GetAudioType(byte[] rawData)
		{
			if (!IsDataUsable(rawData))
				return FmodAudioType.NONE;

			using MemoryStream input = new MemoryStream(rawData);
			using BinaryReader reader = new BinaryReader(input);
			try
			{
				return new FmodAudioHeader(reader).AudioType;
			}
			catch (Exception ex)
			{
				Logger.Warning($"An exception was thrown while attempting to determine the audio type:{Environment.NewLine}{ex.Message}");
				return FmodAudioType.NONE;
			}
		}

		public static string GetFileExtension(IAudioClip audioClip) => GetFileExtension(audioClip.GetAudioData()?.ToArray());
		public static string GetFileExtension(byte[] rawData)
		{
			return GetAudioType(rawData).FileExtension();
		}

		/// <summary>
		/// Not null and at least the minimum size
		/// </summary>
		private static bool IsDataUsable(byte[] data) => data != null && data.Length >= MinimumFsbSize;

		private unsafe static bool IsVorbisLoaded()
		{
			try { OggVorbisSharp.Vorbis.vorbis_version_string(); }
			catch (DllNotFoundException ex)
			{
				Logger.Error($"Could not find vorbis: {ex.Message}");
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
			catch (DllNotFoundException ex)
			{
				Logger.Error($"Could not find ogg: {ex.Message}");
				result = false;
			}
			finally
			{
				Marshal.FreeHGlobal((IntPtr)streamPtr);
			}
			return result;
		}

		private static IntPtr DllImportResolver(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath)
		{
			// On linux, try .so.0
			if (System.OperatingSystem.IsLinux())
			{
				if (libraryName == "ogg" || libraryName == "vorbis")
				{
					return NativeLibrary.Load(libraryName + ".so.0", assembly, searchPath);
				}
			}

			// Otherwise, fallback to default import resolver.
			return IntPtr.Zero;
		}
	}
}
