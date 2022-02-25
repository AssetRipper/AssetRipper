using AssetRipper.Core.Logging;
using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.IO;
using System.Runtime.Versioning;

namespace AssetRipper.Library.Exporters.Audio
{
	public static class AudioConverter
	{
		public static byte[] OggToWav(byte[] oggData)
		{
			if (oggData == null)
				throw new ArgumentNullException(nameof(oggData));
			if (oggData.Length == 0)
				return Array.Empty<byte>();

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

		[SupportedOSPlatform("windows")] //NAudio.Lame is Windows-only
		public static byte[] WavToMp3(byte[] wavData)
		{
			if (wavData == null)
				throw new ArgumentNullException(nameof(wavData));
			if (wavData.Length == 0)
				return Array.Empty<byte>();

			try
			{
				using MemoryStream readStream = new MemoryStream(wavData);
				using WaveFileReader reader = new WaveFileReader(readStream);
				using MemoryStream writeStream = new MemoryStream();
				using NAudio.Lame.LameMP3FileWriter writer = new NAudio.Lame.LameMP3FileWriter(writeStream, reader.WaveFormat, NAudio.Lame.LAMEPreset.STANDARD);
				reader.CopyTo(writer);
				return writeStream.ToArray();
			}
			catch (Exception ex)
			{
				Logger.Error(LogCategory.Export, "Failed to convert audio from WAV to MP3", ex);
				return Array.Empty<byte>();
			}
		}
	}
}
