using AssetRipper.Assets.Metadata;

namespace AssetRipper.Assets;

/// <summary>
/// An object that, if referenced by a <see cref="PPtr"/>, returns null unless explicity searched for.
/// </summary>
public abstract class NullObject : UnityObjectBase
{
	protected NullObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}
}
