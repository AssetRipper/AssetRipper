using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.Library.Exporters.Audio
{
	public sealed class NativeAudioExportCollection : AssetExportCollection
	{
		public NativeAudioExportCollection(NativeAudioExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			IAudioClip audioClip = (IAudioClip)asset;
			if (audioClip.Has_Type_C83())
			{
				return audioClip.GetSoundType().ToRawExtension();
			}
			else
			{
				return audioClip.GetCompressionFormat().ToRawExtension();
			}
		}
	}
}
