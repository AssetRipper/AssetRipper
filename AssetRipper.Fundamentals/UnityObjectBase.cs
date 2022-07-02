using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Layout;
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
	public class UnityObjectBase : UnityAssetBase, IUnityObjectBase
	{
		public AssetInfo AssetInfo { get; set; }
		public ISerializedFile SerializedFile => AssetInfo.File;
		public virtual ClassIDType ClassID => AssetInfo.ClassID;
		public virtual string AssetClassName => "Unknown";
		public long PathID => AssetInfo.PathID;
		public UnityGUID GUID
		{
			get => AssetInfo.GUID;
			set
			{
				AssetInfo.GUID = value;
			}
		}
		public virtual string ExportPath => Path.Combine(AssetsKeyword, AssetClassName);
		public virtual string ExportExtension => AssetExtension;

		public const string AssetsKeyword = "Assets";
		protected const string AssetExtension = "asset";

		public UnityObjectBase() : base()
		{
			AssetInfo = AssetInfo.MakeDummyAssetInfo();
		}

		public UnityObjectBase(LayoutInfo layout) : base(layout) 
		{
			AssetInfo = AssetInfo.MakeDummyAssetInfo();
		}

		public UnityObjectBase(AssetInfo assetInfo) : base()
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
	}
}
