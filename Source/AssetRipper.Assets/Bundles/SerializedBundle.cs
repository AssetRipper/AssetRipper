using AssetRipper.Assets.Collections;
using AssetRipper.Assets.IO;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A <see cref="Bundle"/> created from serialized assets.
/// </summary>
public sealed class SerializedBundle : Bundle
{
	private string name = string.Empty;

	public override string Name => name;

	public static SerializedBundle FromFileContainer(FileContainer container, AssetFactoryBase factory, UnityVersion defaultVersion = default)
	{
		SerializedBundle bundle = new();
		bundle.name = container.NameFixed;
		foreach (ResourceFile resourceFile in container.ResourceFiles)
		{
			bundle.AddResource(resourceFile);
		}
		foreach (SerializedFile serializedFile in container.SerializedFiles)
		{
			bundle.AddCollectionFromSerializedFile(serializedFile, factory, defaultVersion);
		}
		foreach (FileContainer childContainer in container.FileLists)
		{
			SerializedBundle childBundle = FromFileContainer(childContainer, factory, defaultVersion);
			bundle.AddBundle(childBundle);
		}
		foreach (FailedFile failedFile in container.FailedFiles)
		{
			bundle.AddFailed(failedFile);
		}
		return bundle;
	}

	protected override bool IsCompatibleBundle(Bundle bundle) => bundle is SerializedBundle;

	protected override bool IsCompatibleCollection(AssetCollection collection) => collection is SerializedAssetCollection;
}
