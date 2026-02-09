using AssetRipper.Assets.Collections;
using System.Diagnostics;

namespace AssetRipper.Assets.Debugging;

[DebuggerTypeProxy(typeof(UnityAssetBaseProxy))]
internal readonly struct UnityAssetBaseWithCollection
{
	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	internal IUnityAssetBase Asset { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	internal AssetCollection Collection { get; }

	public UnityAssetBaseWithCollection(IUnityAssetBase asset, AssetCollection collection)
	{
		Asset = asset;
		Collection = collection;
	}

	public override string? ToString()
	{
		return Asset.ToString();
	}
}
