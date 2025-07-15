using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Debugging;
using AssetRipper.Assets.Metadata;
using System.Diagnostics;

namespace AssetRipper.Assets;

/// <summary>
/// The artificial base class for all generated Unity classes which inherit from Object.
/// </summary>
[DebuggerTypeProxy(typeof(UnityObjectBaseProxy))]
public abstract partial class UnityObjectBase : UnityAssetBase, IUnityObjectBase
{
	protected UnityObjectBase(AssetInfo assetInfo)
	{
		AssetInfo = assetInfo;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private PathDetails? originalPathDetails;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private PathDetails? overridePathDetails;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public AssetInfo AssetInfo { get; }
	public AssetCollection Collection => AssetInfo.Collection;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public int ClassID => AssetInfo.ClassID;
	public long PathID => AssetInfo.PathID;
	public virtual string ClassName => GetType().Name;
	public IUnityObjectBase? MainAsset { get; set; }

	/// <inheritdoc/>
	public string? OriginalPath
	{
		get => originalPathDetails?.ToString();
		set
		{
			if (value is null)
			{
				originalPathDetails = null;
			}
			else
			{
				originalPathDetails ??= new();
				originalPathDetails.FullPath = value;
			}
		}
	}

	/// <inheritdoc/>
	public string? OriginalDirectory
	{
		get => originalPathDetails?.Directory;
		set
		{
			if (originalPathDetails is not null)
			{
				originalPathDetails.Directory = value;
			}
			else if (value is not null)
			{
				originalPathDetails = new();
				originalPathDetails.Directory = value;
			}
		}
	}

	/// <inheritdoc/>
	public string? OriginalName
	{
		get => originalPathDetails?.Name;
		set
		{
			if (originalPathDetails is not null)
			{
				originalPathDetails.Name = value;
			}
			else if (value is not null)
			{
				originalPathDetails = new();
				originalPathDetails.Name = value;
			}
		}
	}

	/// <inheritdoc/>
	public string? OriginalExtension
	{
		get => originalPathDetails?.Extension;
		set
		{
			if (originalPathDetails is not null)
			{
				originalPathDetails.Extension = value;
			}
			else if (value is not null)
			{
				originalPathDetails = new();
				originalPathDetails.Extension = value;
			}
		}
	}

	/// <inheritdoc/>
	public string? OverridePath
	{
		get => overridePathDetails?.ToString();
		set
		{
			if (value is null)
			{
				overridePathDetails = null;
			}
			else
			{
				overridePathDetails ??= new();
				overridePathDetails.FullPath = value;
			}
		}
	}

	/// <inheritdoc/>
	public string? OverrideDirectory
	{
		get => overridePathDetails?.Directory;
		set
		{
			if (overridePathDetails is not null)
			{
				overridePathDetails.Directory = value;
			}
			else if (value is not null)
			{
				overridePathDetails = new();
				overridePathDetails.Directory = value;
			}
		}
	}

	/// <inheritdoc/>
	public string? OverrideName
	{
		get => overridePathDetails?.Name;
		set
		{
			if (overridePathDetails is not null)
			{
				overridePathDetails.Name = value;
			}
			else if (value is not null)
			{
				overridePathDetails = new();
				overridePathDetails.Name = value;
			}
		}
	}

	/// <inheritdoc/>
	public string? OverrideExtension
	{
		get => overridePathDetails?.Extension;
		set
		{
			if (overridePathDetails is not null)
			{
				overridePathDetails.Extension = value;
			}
			else if (value is not null)
			{
				overridePathDetails = new();
				overridePathDetails.Extension = value;
			}
		}
	}

	/// <summary>
	/// The name of the asset bundle containing this asset.
	/// </summary>
	public string? AssetBundleName { get; set; }
}
