using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Utils;
using System;

namespace AssetRipper.Library.Exporters.Audio
{
	public static class TempAudioFileCreator
	{
		private const string WavExtension = ".wav";

		/// <summary>
		/// Creates a temporary audio file in the wav format
		/// </summary>
		/// <param name="audioClip">The audio clip to be decoded</param>
		/// <returns>
		/// The full path to created temporary audio file
		/// </returns>
		public static string CreateTempAudioFile(AudioClip audioClip)
		{
			if (audioClip == null)
				throw new ArgumentNullException(nameof(audioClip));

			byte[] decodedData = AudioClipDecoder.GetDecodedAudioClipData(audioClip);

			if (decodedData == null)
				return null;

			decodedData = AudioClipExporter.ConvertToWav(decodedData);

			return TempFolderManager.WriteToTempFile(decodedData, WavExtension);
		}
	}
}
