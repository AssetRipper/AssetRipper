using AssetRipper.Import.Logging;
using NAudio.Vorbis;
using NAudio.Wave;

namespace AssetRipper.Export.UnityProjects.Audio
{
	public static class AudioConverter
	{
		public static byte[] OggToWav(byte[] oggData)
		{
			if (oggData == null)
			{
				throw new ArgumentNullException(nameof(oggData));
			}

			if (oggData.Length == 0)
			{
				return Array.Empty<byte>();
			}

			try
			{
				using VorbisWaveReader vorbisStream = new VorbisWaveReader(new MemoryStream(oggData), true);
				using MemoryStream writeStream = new MemoryStream();
				WaveFileWriter.WriteWavFileToStream(writeStream, vorbisStream);
				return writeStream.ToArray();
			}
			catch (Exception ex)
			{
				Logger.Error(LogCategory.Export, "Failed to convert audio from OGG to WAV", ex);
				return Array.Empty<byte>();
			}
		}
	}
}
