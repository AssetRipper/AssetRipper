using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.Export.PrimaryContent.Audio;

public sealed class AudioContentExtractor : IContentExtractor
{
	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		if (asset is IAudioClip audioClip && AudioExportCollection.TryCreate(this, audioClip, out AudioExportCollection? audioExportCollection))
		{
			exportCollection = audioExportCollection;
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}
}
