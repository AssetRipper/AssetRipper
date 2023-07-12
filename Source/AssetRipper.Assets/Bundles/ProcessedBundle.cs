using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files;
using AssetRipper.Primitives;

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
		Name = $"{nameof(ProcessedBundle)}_{UnityGuid.NewGuid()}";
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ProcessedBundle"/> class with the specified name.
	/// </summary>
	/// <param name="name">The name of the bundle.</param>
	public ProcessedBundle(string name)
	{
		ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
		Name = name;
	}
}
