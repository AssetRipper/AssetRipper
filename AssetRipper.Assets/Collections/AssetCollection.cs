using AssetRipper.Assets.Bundles;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Assets.Collections;

/// <summary>
/// A collection of <see cref="UnityObjectBase"/> assets.
/// </summary>
public abstract class AssetCollection
{
	protected AssetCollection(Bundle bundle)
	{
		Bundle = bundle;
	}

	public Bundle Bundle { get; }
	
	private protected readonly Dictionary<long, IUnityObjectBase> m_assets = new();

	public IUnityObjectBase GetAsset(long pathID)
	{
		return TryGetAsset(pathID, out IUnityObjectBase? asset) 
			? asset 
			: throw new Exception($"Object with path ID {pathID} wasn't found");
	}

	public IUnityObjectBase? TryGetAsset(long pathID)
	{
		TryGetAsset(pathID, out IUnityObjectBase? asset);
		return asset;
	}

	public bool TryGetAsset(long pathID, [NotNullWhen(true)] out IUnityObjectBase? asset)
	{
		return m_assets.TryGetValue(pathID, out asset);
	}
}
