using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.Export.UnityProjects.Audio
{
	public sealed class AudioClipExportCollection : AudioExportCollection
	{
		public AudioClipExportCollection(AudioClipExporter assetExporter, IAudioClip asset) : base(assetExporter, asset)
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
