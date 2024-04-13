using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.NativeEnums.Fmod;
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
			else if (CheckMagic(rawData, "FSB5"u8))
			{
				FmodAudioType audioType = (FmodAudioType)uint.MaxValue;
				try
				{
					if (FsbLoader.TryLoadFsbFromByteArray(rawData, out FmodSoundBank? fsbData))
					{
						audioType = fsbData!.Header.AudioType;
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
				}
				catch (Exception ex)
				{
					decodedData = null;
					fileExtension = null;
					message = $"Failed to convert audio ({audioType})\n{ex}";
					return false;
				}
			}
			else if (CheckMagic(rawData, "IMPM"u8))
			{
				fileExtension = FmodSoundType.It.ToRawExtension();
				decodedData = rawData;
				message = null;
				return true;
			}
			else if (CheckMagic(rawData, "Extended Module: "u8))
			{
				fileExtension = FmodSoundType.Xm.ToRawExtension();
				decodedData = rawData;
				message = null;
				return true;
			}
			// https://moddingwiki.shikadi.net/wiki/S3M_Format
			else if (CheckMagic(rawData, "SCRM"u8, 156))
			{
				fileExtension = FmodSoundType.S3m.ToRawExtension();
				decodedData = rawData;
				message = null;
				return true;
			}
			// https://www.aes.id.au/modformat.html
			else if (
				CheckMagic(rawData, "M.K."u8, 1080) ||
				CheckMagic(rawData, "M!K!"u8, 1080) ||
				CheckMagic(rawData, "FLT4"u8, 1080) ||
				CheckMagic(rawData, "FLT8"u8, 1080) ||
				CheckMagic(rawData, "4CHN"u8, 1080) ||
				CheckMagic(rawData, "6CHN"u8, 1080) ||
				CheckMagic(rawData, "8CHN"u8, 1080))
			{
				fileExtension = FmodSoundType.Mod.ToRawExtension();
				decodedData = rawData;
				message = null;
				return true;
			}

			decodedData = null;
			fileExtension = null;
			message = "Failed to convert audio";
			return false;
		}

		private static bool CheckMagic(byte[] data, ReadOnlySpan<byte> magic, int startIndex = 0)
		{
			if (data.Length < magic.Length + startIndex)
			{
				return false;
			}
			return data.AsSpan(startIndex, magic.Length).SequenceEqual(magic);
		}
	}
}
