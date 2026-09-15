using AssetRipper.Assets;

namespace AssetRipper.Processing.Extended.Deduplication;

/// <summary>
/// Maps each redundant asset to the canonical asset that replaces it.
/// </summary>
/// <remarks>
/// Written during processing and read during export, so one instance is shared by the
/// processor and the exporter that the handler owns.
/// </remarks>
public sealed class DeduplicationMap
{
	private readonly Dictionary<IUnityObjectBase, IUnityObjectBase> replacements = new(ReferenceEqualityComparer.Instance);

	public int Count => replacements.Count;

	public void Add(IUnityObjectBase duplicate, IUnityObjectBase canonical)
	{
		ArgumentNullException.ThrowIfNull(duplicate);
		ArgumentNullException.ThrowIfNull(canonical);
		if (ReferenceEquals(duplicate, canonical))
		{
			throw new ArgumentException("An asset cannot replace itself.", nameof(canonical));
		}
		replacements[duplicate] = canonical;
	}

	public bool TryGetCanonical(IUnityObjectBase asset, [NotNullWhen(true)] out IUnityObjectBase? canonical)
	{
		return replacements.TryGetValue(asset, out canonical);
	}

	public void Clear() => replacements.Clear();
}
