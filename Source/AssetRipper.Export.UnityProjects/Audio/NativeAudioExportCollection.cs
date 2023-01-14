using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Audio
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
