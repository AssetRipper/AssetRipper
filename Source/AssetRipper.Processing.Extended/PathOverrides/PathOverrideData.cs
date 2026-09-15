namespace AssetRipper.Processing.Extended.PathOverrides;

/// <summary>
/// User-supplied export destinations, keyed by collection name and then by path ID.
/// </summary>
public sealed class PathOverrideData
{
	/// <summary>
	/// Outer key: <see cref="AssetRipper.Assets.Collections.AssetCollection.Name"/>.
	/// Inner key: <see cref="AssetRipper.Assets.IUnityObjectBase.PathID"/>.
	/// Value: a path relative to the project root.
	/// </summary>
	public Dictionary<string, Dictionary<long, string>> Files { get; set; } = [];
}
