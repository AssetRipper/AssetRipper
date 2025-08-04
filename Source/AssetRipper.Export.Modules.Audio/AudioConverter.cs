using AssetRipper.Import.Logging;
using NAudio.Vorbis;
using NAudio.Wave;

namespace AssetRipper.Export.Modules.Audio;

public static class AudioConverter
{
	public static byte[] OggToWav(byte[] oggData)
	{
		ArgumentNullException.ThrowIfNull(oggData);

		if (oggData.Length == 0)
		{
			return [];
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
			return [];
		}
	}
}
