using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files.ResourceFiles;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A <see cref="Bundle"/> encompassing an entire game.
/// </summary>
public sealed partial class GameBundle : Bundle
{
	/// <summary>
	/// The <see cref="IResourceProvider"/> being used for this bundle.
	/// </summary>
	public IResourceProvider? ResourceProvider { get; set; }

	/// <summary>
	/// The name of this bundle which is 'GameBundle'.
	/// </summary>
	public override string Name => nameof(GameBundle);

	/// <summary>
	/// Returns true if the given bundle is compatible with this bundle.
	/// </summary>
	/// <param name="bundle">The bundle to check compatibility with.</param>
	protected override bool IsCompatibleBundle(Bundle bundle)
	{
		return bundle is not GameBundle;
	}

	/// <summary>
	/// Resolves an external ResourceFile, or returns null if it cannot be found.
	/// </summary>
	/// <param name="originalName">The original name of the ResourceFile.</param>
	protected override ResourceFile? ResolveExternalResource(string originalName)
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
			return base.ResolveExternalResource(originalName);
		}
	}

	[Obsolete($"{nameof(GameBundle)} has no {nameof(Parent)}. Use {nameof(FetchAssets)} instead.", true)]
	public new IEnumerable<IUnityObjectBase> FetchAssetsInHierarchy() => base.FetchAssetsInHierarchy();

	/// <summary>
	/// Initializes all dependency lists.
	/// </summary>
	public new void InitializeAllDependencyLists(IDependencyProvider? dependencyProvider = null) => base.InitializeAllDependencyLists(dependencyProvider);

	/// Returns true if this bundle has any asset collections.
	/// </summary>
	public bool HasAnyAssetCollections()
	{
		return FetchAssetCollections().Any();
	}

	/// <summary>
	/// Adds a new processed asset collection to this bundle.
	/// </summary>
	/// <param name="name">The name of the new asset collection.</param>
	/// <param name="version">The Unity version of the new asset collection.</param>
	public ProcessedAssetCollection AddNewProcessedCollection(string name, UnityVersion version)
	{
		ProcessedAssetCollection processedCollection = new ProcessedAssetCollection(this);
		processedCollection.Name = name;
		processedCollection.SetLayout(version);
		return processedCollection;
	}

	public ProcessedBundle AddNewProcessedBundle(string? name = null)
	{
		ProcessedBundle processedBundle = new ProcessedBundle(name);
		AddBundle(processedBundle);
		return processedBundle;
	}

	/// <summary>
	/// Returns the maximum Unity version of all asset collections in this bundle.
	/// </summary>
	public UnityVersion GetMaxUnityVersion()
	{
		return FetchAssetCollections().Select(t => t.Version).Append(UnityVersion.MinVersion).Max();
	}
}
