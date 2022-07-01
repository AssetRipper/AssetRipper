using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Library.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using System.Runtime.Versioning;

namespace AssetRipper.Library.Exporters.Audio
{
	public sealed class AudioClipExportCollection : AssetExportCollection
	{
		public AudioClipExportCollection(AudioClipExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			string defaultExtension = AudioClipDecoder.GetFileExtension((IAudioClip)asset);
			if (IsWavExtension((AudioClipExporter)AssetExporter, defaultExtension))
			{
				return "wav";
			}
			else if (IsMp3Extension((AudioClipExporter)AssetExporter, defaultExtension))
			{
				return "mp3";
			}
			else
			{
				return defaultExtension;
			}
		}

		[SupportedOSPlatformGuard("windows")]
		internal static bool IsMp3Extension(AudioClipExporter assetExporter, string defaultExtension)
		{
			return assetExporter.AudioFormat == AudioExportFormat.PreferMp3 
				&& OperatingSystem.IsWindows() 
				&& (defaultExtension == "ogg" || defaultExtension == "wav");
		}

		private static bool IsWavExtension(AudioClipExporter assetExporter, string defaultExtension)
		{
			return assetExporter.AudioFormat == AudioExportFormat.PreferWav 
				&& defaultExtension == "ogg";
		}
	}
}
