using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public abstract class Object : IAssetReadable, IYAMLDocExportable
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

		public static bool IsReadHideFlag(Platform platform)
		{
			return platform == Platform.NoTarget;
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
			if (IsReadHideFlag(stream.Platform))
			{
				ObjectHideFlags = stream.ReadUInt32();
			}
		}
		
		/// <summary>
		/// Export object's content in such formats as txt or png
		/// </summary>
		/// <returns>Object's content</returns>
		public virtual void ExportBinary(IAssetsExporter exporter, Stream stream)
		{
			throw new NotSupportedException($"Type {GetType()} doesn't support binary export");
		}

		public YAMLDocument ExportYAMLDocument(IAssetsExporter exporter)
		{
			YAMLDocument document = new YAMLDocument();
			YAMLMappingNode node = ExportYAMLRoot(exporter);
			YAMLMappingNode root = document.CreateMappingRoot();
			root.Tag = ClassID.ToInt().ToString();
			root.Anchor = exporter.GetExportID(this);
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
			return $"{GetType().Name}'s[{PathID}]";
		}

		protected virtual YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_ObjectHideFlags", ObjectHideFlags);
			return node;
		}

		public ISerializedFile File => m_assetInfo.File;
		public ClassIDType ClassID => m_assetInfo.ClassID;
		public bool IsAsset => ClassID.IsAsset();
		public virtual string ExportExtension => "asset";
		public long PathID => m_assetInfo.PathID;
		
		public UtinyGUID GUID
		{
			get
			{
				if(!Config.IsGenerateGUIDByContent && !IsAsset)
				{
					throw new NotSupportedException("GUIDs are supported only for asset files");
				}
				return m_assetInfo.GUID;
			}
		}

		public uint ObjectHideFlags { get; set; }
		
		private readonly AssetInfo m_assetInfo;
	}
}
