using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Objects;

namespace uTinyRipper.Classes
{
	public abstract class Object : IAsset, IYAMLDocExportable, IDependent
	{
		protected Object(AssetInfo assetInfo)
		{
			if (assetInfo == null)
			{
				throw new ArgumentNullException(nameof(assetInfo));
			}
			AssetInfo = assetInfo;
			if (assetInfo.ClassID != ClassID)
			{
				throw new ArgumentException($"Try to initialize '{ClassID}' with '{assetInfo.ClassID}' asset data", nameof(assetInfo));
			}
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

		public void Read(byte[] buffer)
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				using (AssetReader reader = new AssetReader(stream, File.Version, File.Platform, File.Flags))
				{
					Read(reader);

					if (reader.BaseStream.Position != buffer.Length)
					{
						throw new Exception($"Read less {reader.BaseStream.Position} than expected {buffer.Length}");
					}
				}
			}
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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			return ExportYAMLRoot(container);
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

		/// <summary>
		/// Export object's content in such formats as txt or png
		/// </summary>
		public virtual void ExportBinary(IExportContainer container, Stream stream)
		{
			throw new NotSupportedException($"Type {GetType()} doesn't support binary export");
		}

		public IEnumerable<Object> FetchDependencies(bool isLog = false)
		{
			return FetchDependencies(File, isLog);
		}

		public virtual IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public virtual string ToLogString()
		{
			return $"{GetType().Name}[{PathID}]";
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
		public ClassIDType ClassID => AssetInfo.ClassID;
		public virtual string ExportPath => Path.Combine(AssetsKeyword, ClassID.ToString());
		public virtual string ExportExtension => AssetExtension;
		public long PathID => AssetInfo.PathID;		
		public EngineGUID GUID => AssetInfo.GUID;

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
