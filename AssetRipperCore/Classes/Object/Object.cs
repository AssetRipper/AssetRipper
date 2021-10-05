using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Classes.Object
{
	public abstract class Object : IAsset, IDependent
	{
#warning TODO: remove this whole block
		public AssetInfo AssetInfo { get; set; }
		public ISerializedFile File => AssetInfo.File;
		public virtual ClassIDType ClassID => AssetInfo.ClassID;
		public virtual string ExportPath => Path.Combine(AssetsKeyword, ClassID.ToString());
		public virtual string ExportExtension => AssetExtension;
		public long PathID => AssetInfo.PathID;
		public UnityGUID GUID => AssetInfo.GUID;

		public UnityVersion BundleUnityVersion { get; set; }
		public EndianType EndianType { get; set; }
		public HideFlags ObjectHideFlags { get; set; }

		public const string AssetsKeyword = "Assets";
		protected const string AssetExtension = "asset";

		protected Object(AssetLayout layout) { }

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
			BundleUnityVersion = reader.Version;
			EndianType = reader.EndianType;
			if (HasHideFlag(reader.Version, reader.Flags))
			{
				ObjectHideFlags = (HideFlags)reader.ReadUInt32();
			}
		}

		public virtual void Write(AssetWriter writer)
		{
			if (HasHideFlag(writer.Version, writer.Flags))
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
			root.Add(container.ExportLayout.GetClassName(ClassID), node);
			return document;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			return ExportYAMLRoot(container);
		}

		public virtual IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield break;
		}

		protected virtual YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			if (HasHideFlag(container.Version,container.Flags))
			{
				node.Add(ObjectHideFlagsName, (uint)ObjectHideFlags);
			}
			return node;
		}

		/// <summary>
		/// greater than 2.0.0 and Not Release
		/// </summary>
		public static bool HasHideFlag(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(2) && !flags.IsRelease();

		public const string ObjectHideFlagsName = "m_ObjectHideFlags";
		public const string InstanceIDName = "m_InstanceID";
		public const string LocalIdentfierInFileName = "m_LocalIdentfierInFile";
	}
}
