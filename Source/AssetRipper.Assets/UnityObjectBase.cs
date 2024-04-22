using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using System.Diagnostics;

namespace AssetRipper.Assets;

/// <summary>
/// The artificial base class for all generated Unity classes which inherit from Object.
/// </summary>
public abstract class UnityObjectBase : UnityAssetBase, IUnityObjectBase
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
	/// Get the best name for this object.
	/// </summary>
	/// <remarks>
	/// In order of preference:<br/>
	/// 1. <see cref="IHasNameString.NameString"/><br/>
	/// 2. <see cref="OriginalName"/><br/>
	/// 3. <see cref="ClassName"/><br/>
	/// <see cref="OriginalName"/> has secondary preference because file importers can create assets with a different name from the file.
	/// </remarks>
	/// <returns>A nonempty string.</returns>
	public string GetBestName()
	{
		string? name = (this as INamed)?.Name;
		if (!string.IsNullOrEmpty(name))
		{
			return name;
		}
		else if (!string.IsNullOrEmpty(OriginalName))
		{
			return OriginalName;
		}
		else
		{
			return ClassName;
		}
	}

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

	private sealed class OriginalPathDetails
	{
		private string? directory;
		private string? name;
		private string? extension;
		private string? fullPath;

		public string? Directory
		{
			get => directory;
			set
			{
				directory = value;
				fullPath = CalculatePath();
			}
		}

		public string? Name
		{
			get => name;
			set
			{
				name = value;
				fullPath = CalculatePath();
			}
		}

		/// <summary>
		/// Not including the period
		/// </summary>
		public string? Extension
		{
			get => extension;
			set
			{
				extension = RemovePeriod(value);
				fullPath = CalculatePath();
			}
		}

		public string? FullPath
		{
			get => fullPath;
			set
			{
				if (value != fullPath)
				{
					fullPath = value;
					Directory = Path.GetDirectoryName(value);
					Name = Path.GetFileNameWithoutExtension(value);
					Extension = RemovePeriod(Path.GetExtension(value));
				}
			}
		}

		private string NameWithExtension => string.IsNullOrEmpty(Extension) ? Name ?? "" : $"{Name}.{Extension}";

		public override string? ToString() => FullPath;

		private string CalculatePath()
		{
			return Directory is null
				? NameWithExtension
				: Path.Combine(Directory, NameWithExtension);
		}

		[return: NotNullIfNotNull(nameof(str))]
		private static string? RemovePeriod(string? str)
		{
			return string.IsNullOrEmpty(str) || str[0] != '.' ? str : str.Substring(1);
		}
	}
}
