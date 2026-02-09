using AssetRipper.Assets;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Audio;

public class NativeAudioExporter : BinaryAssetExporter
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset is IAudioClip clip)
		{
			exportCollection = new NativeAudioExportCollection(this, clip);
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
		IAudioClip audioClip = (IAudioClip)asset;

		byte[] data = audioClip.GetAudioData();
		if (data.Length == 0)
		{
			Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{audioClip.GetBestName()}' because no valid data was found");
			return false;
		}
		else
		{
			fileSystem.File.WriteAllBytes(path, data);
			return true;
		}
	}
}
