using AssetRipper.Core;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using FMOD;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AssetRipper.Library.Exporters.Audio
{
	public class FmodAudioExporter : BinaryAssetExporter
	{
		private AudioExportFormat AudioFormat { get; set; }
		public FmodAudioExporter(LibraryConfiguration configuration) => AudioFormat = configuration.AudioExportFormat;

		public static byte[] ExportWavAudio(IAudioClip audioClip)
		{
			byte[] data = audioClip.GetAudioData();
			if (data.Length == 0)
			{
				return null;
			}
			return ConvertToWav(data);
		}

		public static bool IsSupported(IAudioClip audioClip)
		{
			if (audioClip.HasType)
			{
				switch (audioClip.Type)
				{
					case FMODSoundType.AIFF:
					case FMODSoundType.IT:
					case FMODSoundType.MOD:
					case FMODSoundType.S3M:
					case FMODSoundType.XM:
					case FMODSoundType.XMA:
					case FMODSoundType.VAG:
					case FMODSoundType.AUDIOQUEUE:
						return true;
					default:
						return false;
				}
			}
			else
			{
				switch (audioClip.CompressionFormat)
				{
					case AudioCompressionFormat.PCM:
					case AudioCompressionFormat.Vorbis:
					case AudioCompressionFormat.ADPCM:
					case AudioCompressionFormat.MP3:
					case AudioCompressionFormat.VAG:
					case AudioCompressionFormat.HEVAG:
					case AudioCompressionFormat.XMA:
					case AudioCompressionFormat.GCADPCM:
					case AudioCompressionFormat.ATRAC9:
						return true;
					default:
						return false;
				}
			}
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return AudioFormat != AudioExportFormat.Native && asset is IAudioClip audio && IsSupported(audio);
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			if (AudioFormat == AudioExportFormat.Mp3 && OperatingSystem.IsWindows())
				return new AssetExportCollection(this, asset, Mp3Extension);
			else
				return new AssetExportCollection(this, asset, WavExtension);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			IAudioClip audioClip = (IAudioClip)asset;
			if (!audioClip.CheckAssetIntegrity())
			{
				Logger.Warning(LogCategory.Export, $"Can't export '{audioClip.Name}' because resources file hasn't been found");
				return false;
			}

			byte[] data = ExportWavAudio(audioClip);
			if (data == null)
			{
				Logger.Warning(LogCategory.Export, $"Unable to convert '{audioClip.GetValidName()}' to wav");
				return false;
			}

			if (AudioFormat == AudioExportFormat.Mp3 && OperatingSystem.IsWindows())
				data = AudioConverter.WavToMp3(data);

			TaskManager.AddTask(File.WriteAllBytesAsync(path, data));
			return true;
		}

		private static byte[] ConvertToWav(byte[] fmodData)
		{
			RESULT result = Factory.System_Create(out FMOD.System system);
			if (result != RESULT.OK)
			{
				return null;
			}

			try
			{
				result = system.init(1, INITFLAGS.NORMAL, IntPtr.Zero);
				if (result != RESULT.OK)
				{
					return null;
				}

				CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
				exinfo.cbsize = Marshal.SizeOf(exinfo);
				exinfo.length = (uint)fmodData.Length;
				result = system.createSound(fmodData, MODE.OPENMEMORY, ref exinfo, out Sound sound);
				if (result != RESULT.OK)
				{
					return null;
				}

				try
				{
					result = sound.getSubSound(0, out Sound subsound);
					if (result != RESULT.OK)
					{
						return null;
					}

					try
					{
						result = subsound.getFormat(out SOUND_TYPE type, out SOUND_FORMAT format, out int numChannels, out int bitsPerSample);
						if (result != RESULT.OK)
						{
							return null;
						}

						result = subsound.getDefaults(out float frequency, out int priority);
						if (result != RESULT.OK)
						{
							return null;
						}

						int sampleRate = (int)frequency;
						result = subsound.getLength(out uint length, TIMEUNIT.PCMBYTES);
						if (result != RESULT.OK)
						{
							return null;
						}

						result = subsound.@lock(0, length, out IntPtr ptr1, out IntPtr ptr2, out uint len1, out uint len2);
						if (result != RESULT.OK)
						{
							return null;
						}

						const int WavHeaderLength = 44;
						int bufferLen = (int)(WavHeaderLength + len1);
						byte[] buffer = new byte[bufferLen];
						using (MemoryStream stream = new MemoryStream(buffer))
						{
							using BinaryWriter writer = new BinaryWriter(stream);
							writer.Write(RiffFourCC);
							writer.Write(36 + len1);
							writer.Write(WaveEightCC);
							writer.Write(16);
							writer.Write((short)1);
							writer.Write((short)numChannels);
							writer.Write(sampleRate);
							writer.Write(sampleRate * numChannels * bitsPerSample / 8);
							writer.Write((short)(numChannels * bitsPerSample / 8));
							writer.Write((short)bitsPerSample);
							writer.Write(DataFourCC);
							writer.Write(len1);
						}
						Marshal.Copy(ptr1, buffer, WavHeaderLength, (int)len1);
						subsound.unlock(ptr1, ptr2, len1, len2);
						return buffer;
					}
					finally
					{
						subsound.release();
					}
				}
				finally
				{
					sound.release();
				}
			}
			finally
			{
				system.release();
			}
		}

		/// <summary>
		/// 'RIFF' ascii
		/// </summary>
		private const uint RiffFourCC = 0x46464952;
		/// <summary>
		/// 'WAVEfmt ' ascii
		/// </summary>
		private const ulong WaveEightCC = 0x20746D6645564157;
		/// <summary>
		/// 'data' ascii
		/// </summary>
		private const uint DataFourCC = 0x61746164;
		private const string Mp3Extension = "mp3";
		private const string WavExtension = "wav";
	}
}