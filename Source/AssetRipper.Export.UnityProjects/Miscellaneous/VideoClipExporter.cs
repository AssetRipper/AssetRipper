using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_329;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Miscellaneous;

public sealed class VideoClipExporter : BinaryAssetExporter
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset is IVideoClip clip && clip.ExternalResources.CheckIntegrity(clip.Collection))
		{
			exportCollection = new VideoClipExportCollection(this, clip);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}

	public override bool Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		if (((IVideoClip)asset).TryGetContent(out byte[]? data))
		{
			fileSystem.File.WriteAllBytes(path, data);
			return true;
		}
		else
		{
			return false;
		}
	}
}
