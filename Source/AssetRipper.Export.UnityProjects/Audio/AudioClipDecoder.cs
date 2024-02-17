using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Extensions;
using Fmod5Sharp;
using Fmod5Sharp.FmodTypes;
using Fmod5Sharp.Util;

namespace AssetRipper.Export.UnityProjects.Audio
{
	public static class AudioClipDecoder
	{
		public static bool TryDecode(
			IAudioClip audioClip,
			[NotNullWhen(true)] out byte[]? decodedData,
			[NotNullWhen(true)] out string? fileExtension,
			[NotNullWhen(false)] out string? message)
		{
			byte[] rawData = audioClip.GetAudioData();

			if (rawData.Length == 0)
			{
				decodedData = null;
				fileExtension = null;
				message = $"Can't decode audio clip '{audioClip.Name}' with default decoder because its audio data could not be found.";
				return false;
			}

			if (audioClip.Has_Type())
			{
				fileExtension = audioClip.GetSoundType().ToRawExtension();
				decodedData = rawData;
				message = null;
				return true;
			}
			else if (CheckMagic(rawData, "FSB5"u8) && FsbLoader.TryLoadFsbFromByteArray(rawData, out FmodSoundBank? fsbData))
			{
				FmodAudioType audioType = fsbData!.Header.AudioType;
				try
				{
					if (audioType.IsSupported() && fsbData.Samples.Single().RebuildAsStandardFileFormat(out decodedData, out fileExtension))
					{
						message = null;
						return true;
					}
					else
					{
						decodedData = null;
						fileExtension = null;
						message = $"Can't decode audio clip '{audioClip.Name}' with default decoder because it's '{audioType}' encoded.";
						return false;
					}
				}
				catch (Exception ex)
				{
					decodedData = null;
					fileExtension = null;
					message = $"Failed to convert audio ({Enum.GetName(audioType)})\n{ex}";
					return false;
				}
			}
			else
			{
				decodedData = null;
				fileExtension = null;
				message = "Failed to convert audio";
				return false;
			}
		}

		private static bool CheckMagic(byte[] data, ReadOnlySpan<byte> magic)
		{
			if (data.Length < magic.Length)
			{
				return false;
			}
			return data.AsSpan(0, magic.Length).SequenceEqual(magic);
		}
	}
}
