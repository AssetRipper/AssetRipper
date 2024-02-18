using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A <see cref="Bundle"/> containing <see cref="ProcessedAssetCollection"/>s.
/// </summary>
public sealed class ProcessedBundle : VirtualBundle<ProcessedAssetCollection>
{
	/// <inheritdoc/>
	public override string Name { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ProcessedBundle"/> class with a generated name.
	/// </summary>
	public ProcessedBundle()
	{
		Name = GenerateRandomName();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ProcessedBundle"/> class.
	/// </summary>
	/// <param name="name">The name of the bundle. If a name is not provided, a random name is generated.</param>
	public ProcessedBundle(string? name)
	{
		Name = string.IsNullOrEmpty(name) ? GenerateRandomName() : name;
	}

	private static string GenerateRandomName() => $"{nameof(ProcessedBundle)}_{UnityGuid.NewGuid()}";

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
}
