using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Interfaces;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.VersionUtilities;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A <see cref="Bundle"/> encompassing an entire game.
/// </summary>
public sealed partial class GameBundle : Bundle
{
	public IResourceProvider? ResourceProvider { get; set; }
	public IReadOnlyList<TemporaryBundle> TemporaryBundles => temporaryBundles;
	private readonly List<TemporaryBundle> temporaryBundles = new();

	public override string Name => nameof(GameBundle);

	protected override bool IsCompatibleBundle(Bundle bundle)
	{
		return bundle is ProcessedBundle or SerializedBundle;
	}

	protected override bool IsCompatibleCollection(AssetCollection collection)
	{
		return collection is SerializedAssetCollection or ProcessedAssetCollection;
	}

	protected override ResourceFile? ResolveExternalResource(string originalName, string fixedName)
	{
		if (ResourceProvider is not null)
		{
			ResourceFile? resourceFile = ResourceProvider.FindResource(originalName);
			if (resourceFile is not null)
			{
				AddResource(resourceFile);
			}
			return resourceFile;
		}
		else
		{
			return base.ResolveExternalResource(originalName, fixedName);
		}
	}

	public void ClearTemporaryBundles()
	{
		temporaryBundles.Clear();
	}

	public void AddTemporaryBundle(TemporaryBundle bundle)
	{
		if (bundle.Parent is null)
		{
			temporaryBundles.Add(bundle);
			bundle.Parent = this;
		}
		else if (bundle.Parent == this)
		{
		}
		else
		{
			throw new ArgumentException($"{nameof(bundle)} already has a parent.", nameof(bundle));
		}
	}

	public TemporaryBundle AddNewTemporaryBundle()
	{
		TemporaryBundle bundle = new();
		temporaryBundles.Add(bundle);
		bundle.Parent = this;
		return bundle;
	}

	public bool HasAnyAssetCollections()
	{
		return FetchAssetCollections().Any();
	}

	public ProcessedAssetCollection AddNewProcessedCollection(string name, UnityVersion version)
	{
		ProcessedAssetCollection processedCollection = new ProcessedAssetCollection(this);
		processedCollection.Name = name;
		processedCollection.SetLayout(version);
		return processedCollection;
	}

	public UnityVersion GetMaxUnityVersion()
	{
		return FetchAssetCollections().Select(t => t.Version).Append(UnityVersion.MinVersion).Max();
	}
}
