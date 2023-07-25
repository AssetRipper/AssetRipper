using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Extensions;
using Fmod5Sharp;
using Fmod5Sharp.FmodTypes;
using Fmod5Sharp.Util;

namespace AssetRipper.Export.UnityProjects.Audio
{
	public static class AudioClipDecoder
	{
		public static bool CanDecode(IAudioClip audioClip)
		{
			byte[] rawData = audioClip.GetAudioData();
			if (rawData.Length == 0)
			{
				Logger.Info(LogCategory.Export, $"Can't decode audio clip '{audioClip.Name}' with default decoder because its audio data could not be found.");
				return false;
			}

			if (TryGetAudioType(rawData, out FmodAudioType audioType))
			{
				if (FmodAudioTypeExtensions.IsSupported(audioType))
				{
					return true;
				}
				else
				{
					Logger.Info(LogCategory.Export, $"Can't decode audio clip '{audioClip.Name}' with default decoder because it's '{audioType}' encoded.");
					return false;
				}
			}
			else
			{
				Logger.Info(LogCategory.Export, $"Can't decode audio clip '{audioClip.Name}' with default decoder because its {nameof(audioType)} could not be determined.");
				return false;
			}
		}

		public static bool TryGetDecodedAudioClipData(IAudioClip audioClip, [NotNullWhen(true)] out byte[]? decodedData, [NotNullWhen(true)] out string? fileExtension)
		{
			return TryGetDecodedAudioClipData(audioClip.GetAudioData(), out decodedData, out fileExtension);
		}
		public static bool TryGetDecodedAudioClipData(byte[] rawData, [NotNullWhen(true)] out byte[]? decodedData, [NotNullWhen(true)] out string? fileExtension)
		{
			decodedData = null;
			fileExtension = null;

			if (rawData.Length == 0)
			{
				return false;
			}

			if (FsbLoader.TryLoadFsbFromByteArray(rawData, out FmodSoundBank? fsbData))
			{
				FmodAudioType audioType = fsbData!.Header.AudioType;
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
			else
			{
				Logger.Error(LogCategory.Export, $"Failed to convert audio");
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
			return TryGetDecodedWavData(audioClip.GetAudioData(), out decodedData);
		}
		/// <summary>
		/// Decodes WAV data from FSB data
		/// </summary>
		/// <param name="fsbData">The data from an FSB file</param>
		/// <param name="decodedData">The decoded data in the wav audio format</param>
		/// <returns>True if the audio could be exported in the wav format</returns>
		public static bool TryGetDecodedWavData(byte[] fsbData, [NotNullWhen(true)] out byte[]? decodedData)
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

		public static bool TryGetAudioType(byte[] rawData, out FmodAudioType type)
		{
			if (rawData.Length == 0)
			{
				type = default;
				return false;
			}

			using MemoryStream input = new MemoryStream(rawData);
			using BinaryReader reader = new BinaryReader(input);
			try
			{
				if (CheckMagic(reader))
				{
					FmodAudioHeader header = new FmodAudioHeader(reader);
					type = header.AudioType;
					return true;
				}
				else
				{
					type = default;
					Logger.Info(LogCategory.Export, "Audio clip data is not an FSB5 binary.");
					return false;
				}
			}
			catch (Exception ex)
			{
				Logger.Warning($"An exception was thrown while attempting to determine the audio type:{Environment.NewLine}{ex.Message}");
				type = default;
				return false;
			}
		}

		public static string GetFileExtension(IAudioClip audioClip) => GetFileExtension(audioClip.GetAudioData());
		public static string GetFileExtension(byte[] rawData)
		{
			if (TryGetAudioType(rawData, out FmodAudioType audioType))
			{
				return audioType.FileExtension() ?? throw new Exception($"No extension for {audioType}");
			}
			else if (rawData.Length == 0)
			{
				throw new Exception($"{nameof(rawData)} was empty.");
			}
			else
			{
				throw new Exception($"{nameof(FmodAudioType)} could not be determined.");
			}
		}

		private static bool CheckMagic(BinaryReader reader)
		{
			const byte magic0 = (byte)'F';
			const byte magic1 = (byte)'S';
			const byte magic2 = (byte)'B';
			const byte magic3 = (byte)'5';

			long initialPosition = reader.BaseStream.Position;

			bool isValid = reader.ReadByte() == magic0 && reader.ReadByte() == magic1 && reader.ReadByte() == magic2 && reader.ReadByte() == magic3;
			reader.BaseStream.Position = initialPosition;
			return isValid;
		}
	}
}
