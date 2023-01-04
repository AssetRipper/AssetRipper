using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Import.Project.Collections;
using AssetRipper.Import.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_240;
using AssetRipper.SourceGenerated.Classes.ClassID_241;
using AssetRipper.SourceGenerated.Classes.ClassID_243;
using AssetRipper.SourceGenerated.Classes.ClassID_245;

namespace AssetRipper.Export.UnityProjects.AudioMixers
{
	public class AudioMixerExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IAudioMixerController or IAudioMixerGroupController or IAudioMixerSnapshotController;
		}
		
		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			// Audio mixer groups and snapshots should be serialized into the audio mixer YAML asset,
			// precisely the same as a regular Unity project would do.
			// So when we encounter audio mixer groups and snapshots, we create an export collection for the audio mixer
			// to which they belong and let the collection export all related assets into one audio mixer YAML asset.
			IAudioMixer? audioMixer = asset switch
			{
				IAudioMixerController mixer => mixer,
				IAudioMixerGroupController group => group.AudioMixer_C243P,
				IAudioMixerSnapshotController snapshot => snapshot.AudioMixer_C245P,
				_ => null,
			};
			return audioMixer is IAudioMixerController audioMixerController
				? new AudioMixerExportCollection(this, virtualFile, audioMixerController)
				: new FailExportCollection(this, asset);
		}
	}
}
