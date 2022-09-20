using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A <see cref="Bundle"/> encompassing an entire game.
/// </summary>
public sealed class GameBundle : Bundle
{
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

	public void ClearTemporaryBundles()
	{
		temporaryBundles.Clear();
	}

	public void AddTemporaryBundle(TemporaryBundle bundle)
	{
		temporaryBundles.Add(bundle);
	}
}
