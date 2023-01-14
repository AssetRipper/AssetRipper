using AssetRipper.Assets.Metadata;

namespace AssetRipper.Assets;

/// <summary>
/// An object that, if referenced by a <see cref="PPtr"/> and a type check fails, returns null.
/// </summary>
public abstract class NullObject : UnityObjectBase
{
	protected NullObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}
}
