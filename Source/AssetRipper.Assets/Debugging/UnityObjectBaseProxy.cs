using AssetRipper.Assets.Collections;
using System.Diagnostics;

namespace AssetRipper.Assets.Debugging;

internal readonly struct UnityObjectBaseProxy
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly IUnityObjectBase asset;

	public AssetCollection Collection => asset.Collection;

	public IUnityObjectBase? MainAsset => asset.MainAsset;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public FieldNameValuePair[] Fields => DebuggingWalker.GetFields(asset, Collection);

	public UnityObjectBaseProxy(IUnityObjectBase asset)
	{
		this.asset = asset;
	}

	public override string? ToString()
	{
		return asset.ToString();
	}
}
