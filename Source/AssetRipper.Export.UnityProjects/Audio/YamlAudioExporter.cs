using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.Export.UnityProjects.Audio;

public sealed class YamlAudioExporter : YamlExporterBase
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		exportCollection = asset switch
		{
			IAudioClip audioClip => new YamlAudioExportCollection(this, audioClip),
			_ => null,
		};
		return exportCollection is not null;
	}
}
