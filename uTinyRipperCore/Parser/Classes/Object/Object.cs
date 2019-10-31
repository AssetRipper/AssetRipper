using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes
{
	public abstract class Object : IAsset, IDependent
	{
		protected Object(AssetInfo assetInfo)
		{
			AssetInfo = assetInfo;
		}

		protected Object(AssetInfo assetInfo, HideFlags hideFlags):
			this(assetInfo)
		{
			ObjectHideFlags = hideFlags;
		}

		/// <summary>
		/// 2.0.0 and greater and Not Release and Not Prefab
		/// </summary>
		public static bool HasHideFlag(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && !flags.IsForPrefab() && version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 4.3.0 and greater and Debug
		/// </summary>
		public static bool HasInstanceID(Version version, TransferInstructionFlags flags) => flags.IsDebug() && version.IsGreaterEqual(4, 3);

		public virtual Object Convert(IExportContainer container)
		{
			return this;
		}

		public virtual void Read(AssetReader reader)
		{
			if (HasHideFlag(reader.Version, reader.Flags))
			{
				ObjectHideFlags = (HideFlags)reader.ReadUInt32();
			}
#if UNIVERSAL
			if (HasInstanceID(reader.Version, reader.Flags))
			{
				InstanceID = reader.ReadInt32();
				LocalIdentfierInFile = reader.ReadInt64();
			}
#endif
		}

		public virtual void Write(AssetWriter writer)
		{
			if (HasHideFlag(writer.Version, writer.Flags))
			{
				writer.Write((uint)ObjectHideFlags);
			}
#if UNIVERSAL
			if (HasInstanceID(writer.Version, writer.Flags))
			{
				writer.Write(InstanceID);
				writer.Write(LocalIdentfierInFile);
			}
#endif
		}

		public YAMLDocument ExportYAMLDocument(IExportContainer container)
		{
			YAMLDocument document = new YAMLDocument();
			YAMLMappingNode root = document.CreateMappingRoot();
			root.Tag = ClassID.ToInt().ToString();
			root.Anchor = container.GetExportID(this).ToString();
			YAMLMappingNode node = ExportYAMLRoot(container);
			root.Add(ClassID.ToString(), node);
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
			if (HasHideFlag(container.ExportVersion, container.ExportFlags))
			{
				node.Add(ObjectHideFlagsName, (uint)ObjectHideFlags);
			}
			return node;
		}

		public AssetInfo AssetInfo { get; }
		public ISerializedFile File => AssetInfo.File;
		public virtual ClassIDType ClassID => AssetInfo.ClassID;
		public virtual string ExportPath => Path.Combine(AssetsKeyword, ClassID.ToString());
		public virtual string ExportExtension => AssetExtension;
		public long PathID => AssetInfo.PathID;
		public GUID GUID => AssetInfo.GUID;

		public HideFlags ObjectHideFlags { get; set; }
#if UNIVERSAL
		public int InstanceID { get; set; }
		public long LocalIdentfierInFile { get; set; }
#endif

		public const string TypelessdataName = "_typelessdata";
		public const string ObjectHideFlagsName = "m_ObjectHideFlags";

		public const string AssetsKeyword = "Assets";
		protected const string AssetExtension = "asset";
	}
}
