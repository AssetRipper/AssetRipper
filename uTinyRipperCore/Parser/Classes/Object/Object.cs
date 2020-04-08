using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public abstract class Object : IAsset, IDependent
	{
		protected Object(AssetLayout layout)
		{
		}

		protected Object(AssetInfo assetInfo)
		{
			AssetInfo = assetInfo;
		}

		public virtual Object Convert(IExportContainer container)
		{
			return this;
		}

		public virtual void Read(AssetReader reader)
		{
			ObjectLayout layout = reader.Layout.Object;
			if (layout.HasHideFlag)
			{
				ObjectHideFlags = (HideFlags)reader.ReadUInt32();
			}
		}

		public virtual void Write(AssetWriter writer)
		{
			ObjectLayout layout = writer.Layout.Object;
			if (layout.HasHideFlag)
			{
				writer.Write((uint)ObjectHideFlags);
			}
		}

		public YAMLDocument ExportYAMLDocument(IExportContainer container)
		{
			YAMLDocument document = new YAMLDocument();
			YAMLMappingNode root = document.CreateMappingRoot();
			root.Tag = ClassID.ToInt().ToString();
			root.Anchor = container.GetExportID(this).ToString();
			YAMLMappingNode node = ExportYAMLRoot(container);
			root.Add(container.ExportLayout.ClassNames[ClassID], node);
			return document;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			return ExportYAMLRoot(container);
		}

		/// <summary>
		/// Export object's content in such formats as txt or png
		/// </summary>
		public virtual void ExportBinary(IExportContainer container, Stream stream)
		{
			throw new NotSupportedException($"Type {GetType()} doesn't support binary export");
		}

		public virtual IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield break;
		}

		protected virtual YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			ObjectLayout layout = container.Layout.Object;
			if (layout.HasHideFlag)
			{
				node.Add(layout.ObjectHideFlagsName, (uint)ObjectHideFlags);
			}
			return node;
		}

#warning TODO: remove this whole block
		public AssetInfo AssetInfo { get; set; }
		public ISerializedFile File => AssetInfo.File;
		public virtual ClassIDType ClassID => AssetInfo.ClassID;
		public virtual string ExportPath => Path.Combine(AssetsKeyword, ClassID.ToString());
		public virtual string ExportExtension => AssetExtension;
		public long PathID => AssetInfo.PathID;
		public UnityGUID GUID => AssetInfo.GUID;

		public HideFlags ObjectHideFlags { get; set; }

		public const string AssetsKeyword = "Assets";
		protected const string AssetExtension = "asset";
	}
}
