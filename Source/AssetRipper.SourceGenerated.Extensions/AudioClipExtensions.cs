using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.NativeEnums.Fmod;

namespace AssetRipper.SourceGenerated.Extensions;

public static class AudioClipExtensions
{
	public static byte[] GetAudioData(this IAudioClip audioClip)
	{
		if (audioClip.Has_AudioData() && audioClip.AudioData.Length > 0)
		{
			return audioClip.AudioData;
		}
		else if (audioClip.Has_Resource())
		{
			return audioClip.Resource.GetContent(audioClip.Collection) ?? Array.Empty<byte>();
		}
		//else if (audioClip.StreamingInfo != null && audioClip.LoadType == (int)Classes.AudioClip.AudioClipLoadType.Streaming)
		//{
		//	return audioClip.StreamingInfo.GetContent(audioClip.SerializedFile) ?? Array.Empty<byte>();
		//}
		else
		{
			return Array.Empty<byte>();
		}
	}

	public static bool CheckAssetIntegrity(this IAudioClip audioClip)
	{
		if (audioClip.Has_AudioData() && audioClip.AudioData.Length > 0)
		{
			return true;
		}
		else if (audioClip.Resource != null)
		{
			return audioClip.Resource.CheckIntegrity(audioClip.Collection);
		}
		//else if (audioClip.StreamingInfo != null && audioClip.LoadType == (int)Classes.AudioClip.AudioClipLoadType.Streaming)
		//{
		//	return audioClip.StreamingInfo.CheckIntegrity(audioClip.SerializedFile);
		//}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Only present when <see cref="IAudioClip.Has_Format"/> is true.
	/// </summary>
	public static FmodSoundFormat GetSoundFormat(this IAudioClip audioClip) => (FmodSoundFormat)audioClip.Format;

	/// <summary>
	/// Only present when <see cref="IAudioClip.Has_Type"/> is true.
	/// </summary>
	public static FmodSoundType GetSoundType(this IAudioClip audioClip) => (FmodSoundType)audioClip.Type;
}
