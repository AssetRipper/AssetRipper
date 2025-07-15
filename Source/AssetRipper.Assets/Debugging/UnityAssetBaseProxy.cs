using AssetRipper.Assets.Collections;
using System.Diagnostics;

namespace AssetRipper.Assets.Debugging;

internal readonly struct UnityAssetBaseProxy
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	internal readonly IUnityAssetBase asset;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	internal readonly AssetCollection collection;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public FieldNameValuePair[] lds => DebuggingWalker.GetFields(asset, collection);

	public UnityAssetBaseProxy(UnityAssetBaseWithCollection asset)
	{
		this.asset = asset.Asset;
		collection = asset.Collection;
	}

	public override string? ToString()
	{
		return asset.ToString();
	}
}
