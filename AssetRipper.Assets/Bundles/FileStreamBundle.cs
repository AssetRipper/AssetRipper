namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A <see cref="Bundle"/> created from a BundleFile, ie a UnityFS asset bundle.
/// </summary>
public class FileStreamBundle : Bundle
{
	public override string Name { get; } = string.Empty;
}
