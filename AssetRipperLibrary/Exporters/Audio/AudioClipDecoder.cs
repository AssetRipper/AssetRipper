using AssetRipper.Core.Logging;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using Fmod5Sharp;
using Fmod5Sharp.FmodTypes;
using Fmod5Sharp.Util;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace AssetRipper.Library.Exporters.Audio
{
	public static class AudioClipDecoder
	{
		/// <summary>
		/// Size of the magic number currently
		/// </summary>
		private const int MinimumFsbSize = 4;

		public static bool CanDecode(IAudioClip audioClip)
		{
			byte[] rawData = audioClip.GetAudioData();
			if (!IsDataUsable(rawData))
			{
				return false;
			}

			FmodAudioType audioType = GetAudioType(rawData);
			if (FmodAudioTypeExtensions.IsSupported(audioType))
			{
				return true;
			}
			else
			{
				Logger.Info(LogCategory.Export, $"Can't decode audio clip '{audioClip.NameString}' with default decoder because it's '{audioType}' encoded.");
				return false;
			}
		}

		public static bool TryGetDecodedAudioClipData(IAudioClip audioClip, [NotNullWhen(true)] out byte[]? decodedData, [NotNullWhen(true)] out string? fileExtension)
		{
			return TryGetDecodedAudioClipData(audioClip?.GetAudioData(), out decodedData, out fileExtension);
		}
		public static bool TryGetDecodedAudioClipData([NotNullWhen(true)] byte[]? rawData, [NotNullWhen(true)] out byte[]? decodedData, [NotNullWhen(true)] out string? fileExtension)
		{
			decodedData = null;
			fileExtension = null;

			if (!IsDataUsable(rawData))
			{
				return false;
			}

			FmodSoundBank fsbData = FsbLoader.LoadFsbFromByteArray(rawData);

			FmodAudioType audioType = fsbData.Header.AudioType;
			try
			{
				if (audioType.IsSupported() && fsbData.Samples.Single().RebuildAsStandardFileFormat(out decodedData, out fileExtension))
				{
					return true;
				}
				else
				{
					return false;
				}
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
		public static bool TryGetDecodedWavData(IAudioClip audioClip, [NotNullWhen(true)] out byte[]? decodedData)
		{
			return TryGetDecodedWavData(audioClip?.GetAudioData(), out decodedData);
		}
		/// <summary>
		/// Decodes WAV data from FSB data
		/// </summary>
		/// <param name="fsbData">The data from an FSB file</param>
		/// <param name="decodedData">The decoded data in the wav audio format</param>
		/// <returns>True if the audio could be exported in the wav format</returns>
		public static bool TryGetDecodedWavData([NotNullWhen(true)] byte[]? fsbData, [NotNullWhen(true)] out byte[]? decodedData)
		{
			if (TryGetDecodedAudioClipData(fsbData, out decodedData, out string? fileExtension))
			{
				if (fileExtension == "ogg")
				{
					decodedData = AudioConverter.OggToWav(decodedData);
					return true;
				}
				else
				{
					return fileExtension == "wav";
				}
			}
			else
			{
				decodedData = null;
				return false;
			}
		}

		public static FmodAudioType GetAudioType(byte[]? rawData)
		{
			if (!IsDataUsable(rawData))
			{
				return FmodAudioType.NONE;
			}

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
		public static string GetFileExtension(byte[]? rawData)
		{
			FmodAudioType audioType = GetAudioType(rawData);
			return audioType.FileExtension() ?? throw new Exception($"No extension for {audioType}");
		}

		/// <summary>
		/// Not null and at least the minimum size
		/// </summary>
		private static bool IsDataUsable([NotNullWhen(true)] byte[]? data) => data is not null && data.Length >= MinimumFsbSize;
	}
}
