using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Library.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

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
			else
			{
				return defaultExtension;
			}
		}

		private static bool IsWavExtension(AudioClipExporter assetExporter, string defaultExtension)
		{
			return assetExporter.AudioFormat == AudioExportFormat.PreferWav
				&& defaultExtension == "ogg";
		}
	}
}
