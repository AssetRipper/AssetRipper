using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using System.Diagnostics;

namespace AssetRipper.Assets;

/// <summary>
/// The artificial base class for all generated Unity classes which inherit from Object.
/// </summary>
public abstract partial class UnityObjectBase : UnityAssetBase, IUnityObjectBase
{
	protected UnityObjectBase(AssetInfo assetInfo)
	{
		AssetInfo = assetInfo;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private OriginalPathDetails? originalPathDetails;
	public AssetInfo AssetInfo { get; }
	public AssetCollection Collection => AssetInfo.Collection;
	public int ClassID => AssetInfo.ClassID;
	public long PathID => AssetInfo.PathID;
	public virtual string ClassName => GetType().Name;
	public IUnityObjectBase? MainAsset { get; set; }

	/// <summary>
	/// The original path of the asset's file, relative to the project root.
	/// </summary>
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

	/// <summary>
	/// The original directory containing the asset's file, relative to the project root.
	/// </summary>
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

	/// <summary>
	/// The original name of the asset's file.
	/// </summary>
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

	/// <summary>
	/// The original extension of the asset's file, not including the period.
	/// </summary>
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

	/// <summary>
	/// The name of the asset bundle containing this asset.
	/// </summary>
	public string? AssetBundleName { get; set; }
}
