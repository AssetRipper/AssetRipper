using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Collections;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using UnityObject = AssetRipper.Core.Classes.Object.Object;
using AssetRipper.Core;
using AssetRipper.Library.Configuration;

namespace AssetRipper.Library.Exporters.Audio
{
	[SupportedOSPlatform("windows")]
	public class AudioAssetExporter : IAssetExporter
	{
		private AudioExportFormat AudioFormat { get; set; }
		public AudioAssetExporter(LibraryConfiguration configuration) => AudioFormat = configuration.AudioExportFormat;

		public static byte[] ExportAudio(AudioClip audioClip)
		{
			byte[] data = (byte[])audioClip.GetAudioData();
			if (data.Length == 0)
			{
				return null;
			}
			return FmodAudioConverter.ConvertToWav(data);
		}

		public static bool IsSupported(AudioClip audioClip)
		{
			if (AudioClip.HasType(audioClip.File.Version))
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

		public bool IsHandle(UnityObject asset, CoreConfiguration options)
		{
			return AudioFormat != AudioExportFormat.Native;
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObject asset)
		{
			return new AudioExportCollection(this, (AudioClip)asset);
		}

		public bool Export(IExportContainer container, UnityObject asset, string path)
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

		public void Export(IExportContainer container, UnityObject asset, string path, Action<IExportContainer, UnityObject, string> callback)
		{
			if (Export(container, asset, path))
			{
				callback?.Invoke(container, asset, path);
			}
		}

		public bool Export(IExportContainer container, IEnumerable<UnityObject> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<UnityObject> assets, string path, Action<IExportContainer, UnityObject, string> callback)
		{
			throw new NotSupportedException();
		}

		public AssetType ToExportType(UnityObject asset)
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
			if (AudioClip.HasType(audioClip.File.Version))
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
