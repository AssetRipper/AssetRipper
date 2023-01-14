using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.Export.UnityProjects.Audio
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
