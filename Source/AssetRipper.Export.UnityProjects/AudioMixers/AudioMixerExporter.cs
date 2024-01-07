using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_241;

namespace AssetRipper.Export.UnityProjects.AudioMixers;

public class AudioMixerExporter : YamlExporterBase
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		// Audio mixer groups and snapshots should be serialized into the audio mixer YAML asset,
		// precisely the same as a regular Unity project would do.
		// So when we encounter audio mixer groups and snapshots, we create an export collection for the audio mixer
		// to which they belong and let the collection export all related assets into one audio mixer YAML asset.

		IAudioMixerController? audioMixer = asset.MainAsset as IAudioMixerController;

		exportCollection = audioMixer is not null
			? new AudioMixerExportCollection(this, audioMixer)
			: new FailExportCollection(this, asset);

		return exportCollection is not null;
	}
}
