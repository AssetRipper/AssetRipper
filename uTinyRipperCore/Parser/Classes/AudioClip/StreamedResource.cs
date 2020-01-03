using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AudioClips
{
	public struct StreamedResource : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool HasSize(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}

		public bool CheckIntegrity(ISerializedFile file)
		{
			if (!IsSet)
			{
				return true;
			}
			if (!HasSize(file.Version))
			{
				// I think they read data by its type for this verison, so I can't even export raw data :/
				return false;
			}

			return file.Collection.FindResourceFile(Source) != null;
		}

		public byte[] GetContent(ISerializedFile file)
		{
			IResourceFile res = file.Collection.FindResourceFile(Source);
			if (res == null)
			{
				return null;
			}
			if (Size == 0)
			{
				return null;
			}

			byte[] data = new byte[Size];
			res.Stream.Position = Offset;
			res.Stream.ReadBuffer(data, 0, data.Length);
			return data;
		}

		public void Read(AssetReader reader)
		{
			Source = reader.ReadString();
			Offset = (long)reader.ReadUInt64();
			if (HasSize(reader.Version))
			{
				Size = (long)reader.ReadUInt64();
			}
			else
			{
				reader.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(SourceName, Source);
			node.Add(OffsetName, Offset);
			node.Add(SizeName, Size);
			return node;
		}

		public bool IsSet => Source != string.Empty;

		public string Source { get; set; }
		public long Offset { get; set; }
		public long Size { get; set; }

		public const string SourceName = "m_Source";
		public const string OffsetName = "m_Offset";
		public const string SizeName = "m_Size";
	}
}
