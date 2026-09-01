using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Processing;

/// <summary>
/// A base class for objects generated during asset processing
/// </summary>
/// <param name="assetInfo"></param>
public abstract class GeneratedObjectBase(AssetInfo assetInfo) : UnityAssetBase, IUnityObjectBase
{
	AssetInfo IUnityObjectBase.AssetInfo => assetInfo;
	int IUnityObjectBase.ClassID => assetInfo.ClassID;
	long IUnityObjectBase.PathID => assetInfo.PathID;

	/// <inheritdoc/>
	public AssetCollection Collection => assetInfo.Collection;

	/// <inheritdoc/>
	public virtual string ClassName => GetType().Name;

	/// <inheritdoc/>
	public virtual string? OriginalPath { get => null; set { } }

	/// <inheritdoc/>
	public virtual string? OriginalDirectory { get => null; set { } }

	/// <inheritdoc/>
	public virtual string? OriginalName { get => null; set { } }

	/// <inheritdoc/>
	public virtual string? OriginalExtension { get => null; set { } }

	/// <inheritdoc/>
	public virtual string? OverridePath { get => null; set { } }

	/// <inheritdoc/>
	public virtual string? OverrideDirectory { get => null; set { } }

	/// <inheritdoc/>
	public virtual string? OverrideName { get => null; set { } }

	/// <inheritdoc/>
	public virtual string? OverrideExtension { get => null; set { } }

	/// <inheritdoc/>
	public virtual string? AssetBundleName { get => null; set { } }

	/// <inheritdoc/>
	public virtual IUnityObjectBase? MainAsset { get => null; set { } }
}
