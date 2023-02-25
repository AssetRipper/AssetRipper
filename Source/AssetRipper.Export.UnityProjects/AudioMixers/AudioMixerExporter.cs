using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_241;
using AssetRipper.SourceGenerated.Classes.ClassID_243;
using AssetRipper.SourceGenerated.Classes.ClassID_245;

namespace AssetRipper.Export.UnityProjects.AudioMixers
{
	public class AudioMixerExporter : YamlExporterBase
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			// Audio mixer groups and snapshots should be serialized into the audio mixer YAML asset,
			// precisely the same as a regular Unity project would do.
			// So when we encounter audio mixer groups and snapshots, we create an export collection for the audio mixer
			// to which they belong and let the collection export all related assets into one audio mixer YAML asset.
			IAudioMixerController? audioMixer = asset switch
			{
				IAudioMixerController mixer => mixer,
				IAudioMixerGroupController group => group.AudioMixer_C243P as IAudioMixerController,
				IAudioMixerSnapshotController snapshot => snapshot.AudioMixer_C245P as IAudioMixerController,
				_ => null,
			};

			exportCollection = audioMixer is not null
				? new AudioMixerExportCollection(this, temporaryFile, audioMixer)
				: new FailExportCollection(this, asset);

			return exportCollection is not null;
		}
	}
}
