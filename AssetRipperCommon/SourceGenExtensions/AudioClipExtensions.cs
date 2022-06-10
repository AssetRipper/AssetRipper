using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Extensions;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AudioClipExtensions
	{
		public static byte[] GetAudioData(this IAudioClip audioClip)
		{
			if (!audioClip.AudioData_C83.IsNullOrEmpty())
			{
				return audioClip.AudioData_C83;
			}
			else if (audioClip.Resource_C83 != null)
			{
				return audioClip.Resource_C83.GetContent(audioClip.SerializedFile) ?? Array.Empty<byte>();
			}
			//else if (audioClip.StreamingInfo_C83 != null && audioClip.LoadType_C83 == (int)Classes.AudioClip.AudioClipLoadType.Streaming)
			//{
			//	return audioClip.StreamingInfo_C83.GetContent(audioClip.SerializedFile) ?? Array.Empty<byte>();
			//}
			else
			{
				return Array.Empty<byte>();
			}
		}

		public static bool CheckAssetIntegrity(this IAudioClip audioClip)
		{
			if (!audioClip.AudioData_C83.IsNullOrEmpty())
			{
				return true;
			}
			else if (audioClip.Resource_C83 != null)
			{
				return audioClip.Resource_C83.CheckIntegrity(audioClip.SerializedFile);
			}
			//else if (audioClip.StreamingInfo != null && audioClip.LoadType_C83 == (int)Classes.AudioClip.AudioClipLoadType.Streaming)
			//{
			//	return audioClip.StreamingInfo.CheckIntegrity(audioClip.SerializedFile);
			//}
			else
			{
				return true;
			}
		}

		public static AudioClipLoadType GetLoadType(this IAudioClip audioClip) => (AudioClipLoadType)audioClip.LoadType_C83;

		public static AudioCompressionFormat GetCompressionFormat(this IAudioClip audioClip) => (AudioCompressionFormat)audioClip.CompressionFormat_C83;

		public static FMODSoundFormat GetSoundFormat(this IAudioClip audioClip) => (FMODSoundFormat)audioClip.Format_C83;

		public static FMODSoundType GetSoundType(this IAudioClip audioClip) => (FMODSoundType)audioClip.Type_C83;
	}
}
