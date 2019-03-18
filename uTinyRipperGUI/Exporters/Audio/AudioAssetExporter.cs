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
		public static bool ExportAudio(IExportContainer container, AudioClip audioClip, Stream exportStream)
		{
			using (MemoryStream memStream = new MemoryStream())
			{
				audioClip.ExportBinary(container, memStream);
				if (memStream.Length == 0)
				{
					return false;
				}

				byte[] data = memStream.ToArray();
				return AudioConverter.ConvertToWav(data, exportStream);
			}
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

		public bool IsHandle(Object asset)
		{
			return true;
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new AudioExportCollection(this, (AudioClip)asset);
		}

		public void Export(IExportContainer container, Object asset, string path)
		{
			Export(container, asset, path, null);
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			AudioClip audioClip = (AudioClip)asset;
			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				if (IsSupported(audioClip))
				{
					bool result = ExportAudio(container, audioClip, fileStream);
					if (!result)
					{
						Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{audioClip.Name}' to wav");
					}
				}
				else
				{
					audioClip.ExportBinary(container, fileStream);
				}
			}
			callback?.Invoke(container, asset, path);
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path)
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
