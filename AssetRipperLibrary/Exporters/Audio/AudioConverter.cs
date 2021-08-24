using NAudio.Vorbis;
using NAudio.Wave;
using System.IO;

namespace AssetRipper.Library.Exporters.Audio
{
	public static class AudioConverter
	{
		public static byte[] ConvertToWav(byte[] oggData)
		{
			using (VorbisWaveReader vorbisStream = new VorbisWaveReader(new MemoryStream(oggData), true))
			{
				using(MemoryStream writeStream = new MemoryStream())
				{
					WaveFileWriter.WriteWavFileToStream(writeStream, vorbisStream);
					return writeStream.ToArray();
				}
			}
		}
	}
}
