using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A <see cref="Bundle"/> containing <see cref="ProcessedAssetCollection"/>s.
/// </summary>
public class ProcessedBundle : VirtualBundle<ProcessedAssetCollection>
{
	public override string Name { get; }

	public ProcessedBundle()
	{
		Name = $"{nameof(ProcessedBundle)}_{UnityGUID.NewGuid()}";
	}

	public ProcessedBundle(string name)
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
	}
}
