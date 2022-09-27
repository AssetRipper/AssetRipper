using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.IO;

namespace AssetRipper.Core
{
	/// <summary>
	/// The artificial base class for all generated Unity classes with Type ID numbers<br/>
	/// In other words, the classes that inherit from Object
	/// </summary>
	public abstract class UnityObjectBase : UnityAssetBase, IUnityObjectBase
	{
		private OriginalPathDetails? originalPathDetails;
		public AssetInfo AssetInfo { get; set; }
		public ISerializedFile SerializedFile => AssetInfo.File;
		public virtual ClassIDType ClassID => AssetInfo.ClassID;
		public abstract string AssetClassName { get; }
		public long PathID => AssetInfo.PathID;
		public UnityGUID GUID
		{
			get => AssetInfo.GUID;
			set
			{
				AssetInfo.GUID = value;
			}
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
					originalPathDetails.Extension = Path.GetExtension(value);
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

				static string? RemovePeriod(string? str)
				{
					return string.IsNullOrEmpty(str) || str[0] != '.' ? str : str.Substring(1);
				}
			}
		}
		public string? AssetBundleName { get; set; }

		public UnityObjectBase() : this(AssetInfo.MakeDummyAssetInfo())
		{
		}

		public UnityObjectBase(AssetInfo assetInfo)
		{
			AssetInfo = assetInfo;
		}

		public YamlDocument ExportYamlDocument(IExportContainer container)
		{
			YamlDocument document = new YamlDocument();
			YamlMappingNode root = document.CreateMappingRoot();
			root.Tag = ClassID.ToInt().ToString();
			root.Anchor = container.GetExportID(this).ToString();
			YamlNode node = ExportYaml(container);
			root.Add(AssetClassName, node);
			return document;
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
}
