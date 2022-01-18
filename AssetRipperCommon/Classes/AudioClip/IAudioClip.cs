using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AudioClip
{
	public interface IAudioClip : IUnityObjectBase, INamedObject
	{
		/// <summary>
		/// Stream previously
		/// </summary>
		AudioClipLoadType LoadType { get; set; }
		AudioCompressionFormat CompressionFormat { get; set; }
		FMODSoundFormat Format { get; set; }
		/// <summary>
		/// SoundType in some versions
		/// </summary>
		FMODSoundType Type { get; set; }
		/// <summary>
		/// 2.6.0 to 5.0.0bx (NOTE: unknown version)
		/// </summary>
		bool HasType { get; }
		byte[] AudioData { get; set; }
		IStreamedResource Resource { get; }
		/// <summary>
		/// Not in the type trees. Depends on LoadType
		/// </summary>
		IStreamingInfo StreamingInfo { get; }
	}

	public static class AudioClipExtensions
	{
		public static byte[] GetAudioData(this IAudioClip audioClip)
		{
			if (!audioClip.AudioData.IsNullOrEmpty())
			{
				return audioClip.AudioData;
			}
			else if (audioClip.Resource != null)
			{
				return audioClip.Resource.GetContent(audioClip.SerializedFile) ?? Array.Empty<byte>();
			}
			else if (audioClip.StreamingInfo != null && audioClip.LoadType == AudioClipLoadType.Streaming)
			{
				return audioClip.StreamingInfo.GetContent(audioClip.SerializedFile) ?? Array.Empty<byte>();
			}
			else
			{
				return Array.Empty<byte>();
			}
		}

		public static bool CheckAssetIntegrity(this IAudioClip audioClip)
		{
			if (!audioClip.AudioData.IsNullOrEmpty())
			{
				return true;
			}
			else if (audioClip.Resource != null)
			{
				return audioClip.Resource.CheckIntegrity(audioClip.SerializedFile);
			}
			else if (audioClip.StreamingInfo != null && audioClip.LoadType == AudioClipLoadType.Streaming)
			{
				return audioClip.StreamingInfo.CheckIntegrity(audioClip.SerializedFile);
			}
			else
			{
				return true;
			}
		}
	}
}
