using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A container for <see cref="AssetCollection"/>s, <see cref="ResourceFile"/>s, and other <see cref="Bundle"/>s.
/// </summary>
public abstract class Bundle
{
	public Bundle? Parent { get; private set; }
	public IReadOnlyList<ResourceFile> Resources => resources;
	private readonly List<ResourceFile> resources = new();
	public IReadOnlyList<AssetCollection> Collections => collections;
	private readonly List<AssetCollection> collections = new();
	public IReadOnlyList<Bundle> Bundles => bundles;
	private readonly List<Bundle> bundles = new();
	public abstract string Name { get; }
	
	public void InitializeAllDependencyLists()
	{
		foreach (AssetCollection collection in Collections)
		{
			if (collection is SerializedAssetCollection serializedAssetCollection)
			{
				serializedAssetCollection.InitializeDependencyList();
			}
		}
		foreach (Bundle bundle in Bundles)
		{
			bundle.InitializeAllDependencyLists();
		}
	}

	public AssetCollection? Resolve(FileIdentifier identifier)
	{
		return Resolve(identifier.GetFilePath());
	}

	public virtual AssetCollection? Resolve(string name)
	{
		//Uniqueness is not guaranteed because of asset bundle variants
		return Collections.FirstOrDefault(c => c.Name == name) ?? Parent?.Resolve(name);
	}

	public void AddResource(ResourceFile resource)
	{
		resources.Add(resource);
	}

	public void AddCollection(AssetCollection collection)
	{
		if (collection.Bundle != this)
		{
			throw new ArgumentException($"Collection's {nameof(AssetCollection.Bundle)} property did not match this.", nameof(collection));
		}
		else if (IsCompatibleCollection(collection))
		{
			collections.Add(collection);
		}
		else
		{
			throw new ArgumentException($"The collection is not compatible with this {nameof(Bundle)}.", nameof(collection));
		}
	}

	public void AddBundle(Bundle bundle)
	{
		if (bundle.Parent is null)
		{
			if (IsCompatibleBundle(bundle))
			{
				bundles.Add(bundle);
				bundle.Parent = this;
			}
			else
			{
				throw new ArgumentException($"Child {nameof(Bundle)} is not compatible with this parent {nameof(Bundle)}.", nameof(bundle));
			}
		}
		else if (bundle.Parent == this)
		{
		}
		else
		{
			throw new ArgumentException($"{nameof(bundle)} already has a parent.", nameof(bundle));
		}
	}

	protected virtual bool IsCompatibleCollection(AssetCollection collection) => true;

	protected virtual bool IsCompatibleBundle(Bundle bundle) => bundle is not GameBundle;

	public override string ToString()
	{
		return Name;
	}

	public SerializedAssetCollection AddCollectionFromSerializedFile(SerializedFile file, AssetFactory factory)
	{
		return SerializedAssetCollection.FromSerializedFile(this, file, factory);
	}
}
