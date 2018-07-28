using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
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

		public static bool IsReadHideFlag(TransferInstructionFlags flags)
		{
			return !flags.IsSerializeGameRelease() && !flags.IsSerializeForPrefabSystem();
		}
		public static bool IsReadInstanceID(TransferInstructionFlags flags)
		{
			return flags.IsUnknown2();
		}

		public void Read(byte[] buffer)
		{
			using (MemoryStream baseStream = new MemoryStream(buffer))
			{
				using (AssetStream stream = new AssetStream(baseStream, File.Version, File.Platform))
				{
					Read(stream);

					if (stream.BaseStream.Position != buffer.Length)
					{
						throw new Exception($"Read less {stream.BaseStream.Position} than expected {buffer.Length}");
					}
				}
			}
		}

		public virtual void Read(AssetStream stream)
		{
			if (IsReadHideFlag(stream.Flags))
			{
				ObjectHideFlags = stream.ReadUInt32();
			}
			if(IsReadInstanceID(stream.Flags))
			{
				int instanceID = stream.ReadInt32();
				long localIdentfierInFile = stream.ReadInt64();
#if DEBUG
				InstanceID = instanceID;
				LocalIdentfierInFile = localIdentfierInFile;
#endif
			}
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
			node.Add("m_ObjectHideFlags", ObjectHideFlags);
			return node;
		}

		public ISerializedFile File => m_assetInfo.File;
		public ClassIDType ClassID => m_assetInfo.ClassID;
		public virtual bool IsValid => true;
		public virtual string ExportName => ClassID.ToString();
		public virtual string ExportExtension => "asset";
		public long PathID => m_assetInfo.PathID;
		
		public EngineGUID GUID
		{
			get
			{
				if(!Config.IsGenerateGUIDByContent && this is Component)
				{
					throw new NotSupportedException("GUIDs aren't supported for components");
				}
				return m_assetInfo.GUID;
			}
		}

		public uint ObjectHideFlags { get; set; }
#if DEBUG
		public int InstanceID { get; private set; }
		public long LocalIdentfierInFile { get; private set; }
#endif

		private readonly AssetInfo m_assetInfo;
	}
}
