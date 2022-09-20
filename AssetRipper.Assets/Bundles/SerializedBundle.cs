using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A <see cref="Bundle"/> created from serialized assets.
/// </summary>
public class SerializedBundle : Bundle
{
	private string name = string.Empty;

	public override string Name => name;

	public static SerializedBundle FromFileContainer(FileContainer container, AssetFactory factory)
	{
		SerializedBundle bundle = new();
		bundle.name = container.NameFixed;
		foreach (ResourceFile resourceFile in container.ResourceFiles)
		{
			bundle.AddResource(resourceFile);
		}
		foreach (SerializedFile serializedFile in container.SerializedFiles)
		{
			bundle.AddCollectionFromSerializedFile(serializedFile, factory);
		}
		foreach (FileContainer childContainer in container.FileLists)
		{
			SerializedBundle childBundle = FromFileContainer(childContainer, factory);
			bundle.AddBundle(childBundle);
		}
		return bundle;
	}

	protected override bool IsCompatibleBundle(Bundle bundle)
	{
		return bundle is SerializedBundle;
	}

	protected override bool IsCompatibleCollection(AssetCollection collection)
	{
		return collection is SerializedAssetCollection;
	}
}
