using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AudioClips
{
	public struct StreamedResource : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadSize(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}

		public void Read(AssetReader reader)
		{
			Source = reader.ReadString();
			Offset = (long)reader.ReadUInt64();
			if (IsReadSize(reader.Version))
			{
				Size = (long)reader.ReadUInt64();
			}
			else
			{
				reader.AlignStream(AlignType.Align4);
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

		public string Source { get; private set; }
		public long Offset { get; private set; }
		public long Size { get; private set; }

		public const string SourceName = "m_Source";
		public const string OffsetName = "m_Offset";
		public const string SizeName = "m_Size";
	}
}
