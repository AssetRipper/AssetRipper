using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
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
		public ISerializedFile SerializedFile => AssetInfo?.File;
		public virtual ClassIDType ClassID => AssetInfo?.ClassID ?? ClassIDType.UnknownType;
		public long PathID => AssetInfo.PathID;
		public UnityGUID GUID
		{
			get => AssetInfo.GUID;
			set => AssetInfo.GUID = value;
		}
		public virtual string ExportPath => Path.Combine(AssetsKeyword, GetType().Name);
		public virtual string ExportExtension => AssetExtension;

		public virtual HideFlags ObjectHideFlags
		{
			get => HideFlags.None;
			set => throw new NotSupportedException();
		}

		public const string AssetsKeyword = "Assets";
		protected const string AssetExtension = "asset";

		public UnityObjectBase() : base() { }
		public UnityObjectBase(LayoutInfo layout) : base(layout) { }
		public UnityObjectBase(AssetInfo assetInfo) : base()
		{
			AssetInfo = assetInfo;
			AssetUnityVersion = assetInfo.File.Version;
			TransferInstructionFlags = assetInfo.File.Flags;
		}

		public YAMLDocument ExportYAMLDocument(IExportContainer container)
		{
			YAMLDocument document = new YAMLDocument();
			YAMLMappingNode root = document.CreateMappingRoot();
			root.Tag = ClassID.ToInt().ToString();
			root.Anchor = container.GetExportID(this).ToString();
			YAMLNode node = ExportYAML(container);
			root.Add(container.ExportLayout.GetClassName(ClassID), node);
			return document;
		}

		public override UnityAssetBase CreateAnother() => new UnityObjectBase();

		protected override void CopyValuesFrom(UnityAssetBase source)
		{
			base.CopyValuesFrom(source);
			UnityObjectBase sourceObject = (UnityObjectBase)source;
			//Todo
		}

		public virtual IUnityObjectBase ConvertLegacy(IExportContainer container)
		{
			return this;
		}

		public virtual void ConvertToEditor()
		{
		}
	}
}
