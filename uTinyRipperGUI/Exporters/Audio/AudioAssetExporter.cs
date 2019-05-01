using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.Classes.AudioClips;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipperGUI.Exporters
{
	public class AudioAssetExporter : IAssetExporter
	{
		public static byte[] ExportAudio(AudioClip audioClip)
		{
			byte[] data = (byte[])audioClip.GetAudioData();
			if (data.Length == 0)
			{
				return null;
			}
			return AudioConverter.ConvertToWav(data);
		}

		public static bool IsSupported(AudioClip audioClip)
		{
			if (AudioClip.IsReadType(audioClip.File.Version))
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

		public bool IsHandle(Object asset, ExportOptions options)
		{
			return true;
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new AudioExportCollection(this, (AudioClip)asset);
		}

		public bool Export(IExportContainer container, Object asset, string path)
		{
			AudioClip audioClip = (AudioClip)asset;
			if (!audioClip.CheckAssetIntegrity())
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{audioClip.Name}' because resources file hasn't been found");
				return false;
			}

			if (IsSupported(audioClip))
			{
				byte[] data = ExportAudio(audioClip);
				if (data == null)
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{audioClip.ValidName}' to wav");
					return false;
				}

				using (Stream fileStream = FileUtils.CreateVirtualFile(path))
				{
					fileStream.Write(data, 0, data.Length);
				}
			}
			else
			{
				using (Stream fileStream = FileUtils.CreateVirtualFile(path))
				{
					audioClip.ExportBinary(container, fileStream);
				}
			}
			return true;
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			if (Export(container, asset, path))
			{
				callback?.Invoke(container, asset, path);
			}
		}

		public bool Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public AssetType ToExportType(Object asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}

		private static string GetAudioType(AudioClip audioClip)
		{
			if (AudioClip.IsReadType(audioClip.File.Version))
			{
				return audioClip.Type.ToString();
			}
			else
			{
				return audioClip.CompressionFormat.ToString();
			}
		}
	}
}
