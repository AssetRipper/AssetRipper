using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Dependencies;
using AssetRipper.Assets.Export.Yaml;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.Yaml;

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

	private OriginalPathDetails? originalPathDetails;
	public AssetInfo AssetInfo { get; }
	public AssetCollection Collection => AssetInfo.Collection;
	public int ClassID => AssetInfo.ClassID;
	public long PathID => AssetInfo.PathID;
	public virtual string ClassName => GetType().Name;
	public UnityGUID GUID { get; } = UnityGUID.NewGuid();
	public IUnityObjectBase? MainAsset { get; set; }

	public YamlDocument ExportYamlDocument(IExportContainer container)
	{
		YamlDocument document = new();
		YamlMappingNode root = document.CreateMappingRoot();
		root.Tag = ClassID.ToString();
		root.Anchor = container.GetExportID(this).ToString();
		root.Add(ClassName, this.ExportYaml(container));
		return document;
	}

	public IEnumerable<(FieldName, PPtr<IUnityObjectBase>)> FetchDependencies()
	{
		return FetchDependencies((FieldName?)null);
	}

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
				originalPathDetails.Directory = Path.GetDirectoryName(value);
				originalPathDetails.Name = Path.GetFileNameWithoutExtension(value);
				originalPathDetails.Extension = RemovePeriod(Path.GetExtension(value));
			}
		}
	}
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
	/// Not including the period
	/// </summary>
	public string? OriginalExtension
	{
		get => originalPathDetails?.Extension;
		set
		{
			if (originalPathDetails is not null)
			{
				originalPathDetails.Extension = RemovePeriod(value);
			}
			else if (value is not null)
			{
				originalPathDetails = new();
				originalPathDetails.Extension = RemovePeriod(value);
			}
		}
	}

	public string? AssetBundleName { get; set; }

	private static string? RemovePeriod(string? str)
	{
		return string.IsNullOrEmpty(str) || str[0] != '.' ? str : str.Substring(1);
	}

	private sealed class OriginalPathDetails
	{
		public string? Directory { get; set; }
		public string? Name { get; set; }
		/// <summary>
		/// Not including the period
		/// </summary>
		public string? Extension { get; set; }
		public string NameWithExtension => string.IsNullOrEmpty(Extension) ? Name ?? "" : $"{Name}.{Extension}";

		public override string? ToString()
		{
			string result = Directory is null
				? NameWithExtension
				: Path.Combine(Directory, NameWithExtension);
			return string.IsNullOrEmpty(result) ? null : result;
		}
	}
}
