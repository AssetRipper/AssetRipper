using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Objects;

namespace uTinyRipper.Classes
{
	public abstract class Object : IAssetReadable, IYAMLDocExportable, IDependent
	{
		protected Object(AssetInfo assetInfo)
		{
			if (assetInfo == null)
			{
				throw new ArgumentNullException(nameof(assetInfo));
			}
			m_assetInfo = assetInfo;
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
		public static bool IsReadHideFlag(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && !flags.IsForPrefab() && version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 4.3.0 and greater and Debug
		/// </summary>
		public static bool IsReadInstanceID(Version version, TransferInstructionFlags flags)
		{
			return flags.IsDebug() && version.IsGreaterEqual(4, 3);
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
			if (IsReadHideFlag(reader.Version, reader.Flags))
			{
				ObjectHideFlags = (HideFlags)reader.ReadUInt32();
			}
#if UNIVERSAL
			if (IsReadInstanceID(reader.Version, reader.Flags))
			{
				InstanceID = reader.ReadInt32();
				LocalIdentfierInFile = reader.ReadInt64();
			}
#endif
		}

		/// <summary>
		/// Export object's content in such formats as txt or png
		/// </summary>
		/// <returns>Object's content</returns>
		public virtual void ExportBinary(IExportContainer container, Stream stream)
		{
			throw new NotSupportedException($"Type {GetType()} doesn't support binary export");
		}

		public YAMLDocument ExportYAMLDocument(IExportContainer container)
		{
			YAMLDocument document = new YAMLDocument();
			YAMLMappingNode node = ExportYAMLRoot(container);
			YAMLMappingNode root = document.CreateMappingRoot();
			root.Tag = ClassID.ToInt().ToString();
			root.Anchor = container.GetExportID(this).ToString();
			root.Add(ClassID.ToString(), node);

			return document;
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
			node.Add(ObjectHideFlagsName, (uint)GetObjectHideFlags(container.Version, container.Flags, container.ExportFlags));
			return node;
		}

		private HideFlags GetObjectHideFlags(Version version, TransferInstructionFlags flags, TransferInstructionFlags exportFlags)
		{
			if (IsReadHideFlag(version, flags))
			{
				return ObjectHideFlags;
			}
			if (ClassID == ClassIDType.GameObject)
			{
				GameObject go = (GameObject)this;
				int depth = go.GetRootDepth();
				return depth > 1 ? HideFlags.HideInHierarchy : HideFlags.None;
			}
			return exportFlags.IsForPrefab() ? HideFlags.HideInHierarchy : ObjectHideFlags;
		}

		public ISerializedFile File => m_assetInfo.File;
		public ClassIDType ClassID => m_assetInfo.ClassID;
		public virtual bool IsValid => true;
		public virtual string ExportPath => Path.Combine(AssetsKeyword, ClassID.ToString());
		public virtual string ExportExtension => AssetExtension;
		public long PathID => m_assetInfo.PathID;
		
		public EngineGUID GUID => m_assetInfo.GUID;

		public HideFlags ObjectHideFlags { get; private set; }
#if UNIVERSAL
		public int InstanceID { get; private set; }
		public long LocalIdentfierInFile { get; private set; }
#endif

		public const string ObjectHideFlagsName = "m_ObjectHideFlags";

		public const string AssetsKeyword = "Assets";
		protected const string AssetExtension = "asset";

		private readonly AssetInfo m_assetInfo;
	}
}
