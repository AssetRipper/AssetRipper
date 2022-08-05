using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A container for <see cref="AssetCollection"/>s and ResourceFiles.
/// </summary>
public abstract class Bundle
{
	private readonly List<AssetCollection> collections = new();
	public IReadOnlyList<AssetCollection> Collections => collections;
	public abstract string Name { get; }
	
	public AssetCollection AddCollection(Func<Bundle, AssetCollection> factory)
	{
		AssetCollection collection = factory.Invoke(this);
		if (collection is null)
		{
			throw new ArgumentException("Collection was null.", nameof(factory));
		}
		if (collection.Bundle != this)
		{
			throw new ArgumentException($"Collection's {nameof(AssetCollection.Bundle)} property did not match this.", nameof(factory));
		}
		collections.Add(collection);
		return collection;
	}
}
