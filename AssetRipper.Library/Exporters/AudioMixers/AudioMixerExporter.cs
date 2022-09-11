using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using AssetRipper.Library.Exporters.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_240;
using AssetRipper.SourceGenerated.Classes.ClassID_241;
using AssetRipper.SourceGenerated.Classes.ClassID_243;
using AssetRipper.SourceGenerated.Classes.ClassID_245;
using AssetRipper.SourceGenerated.Classes.ClassID_28;

namespace AssetRipper.Library.Exporters.AudioMixers
{
	public class AudioMixerExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IAudioMixerController or IAudioMixerGroupController or IAudioMixerSnapshotController;
		}
		
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			// Audio mixer groups and snapshots should be serialized into the audio mixer YAML asset,
			// precisely the same as a regular Unity project would do.
			// So when we encounter audio mixer groups and snapshots, we create an export collection for the audio mixer
			// to which they belong and let the collection export all related assets into one audio mixer YAML asset.
			IAudioMixer? audioMixer = asset switch
			{
				IAudioMixerController mixer => mixer,
				IAudioMixerGroupController group => group.AudioMixer_C243.TryGetAsset(group.SerializedFile),
				IAudioMixerSnapshotController snapshot => snapshot.AudioMixer_C245.TryGetAsset(snapshot.SerializedFile),
				_ => null,
			};
			return audioMixer is IAudioMixerController audioMixerController
				? new AudioMixerExportCollection(this, virtualFile, audioMixerController)
				: new FailExportCollection(this, asset);
		}
	}
}
