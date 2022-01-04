using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.AudioClip
{
	public struct StreamedResource : IStreamedResource
	{
		public void Read(AssetReader reader)
		{
			Source = reader.ReadString();
			Offset = reader.ReadUInt64();
			if (StreamedResourceExtensions.HasSize(reader.Version))
			{
				Size = reader.ReadUInt64();
			}
			else
			{
				reader.AlignStream();
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Source);
			writer.Write(Offset);
			if (StreamedResourceExtensions.HasSize(writer.Version))
			{
				writer.Write(Size);
			}
			else
			{
				writer.AlignStream();
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

		public string Source { get; set; }
		public ulong Offset { get; set; }
		public ulong Size { get; set; }

		public const string SourceName = "m_Source";
		public const string OffsetName = "m_Offset";
		public const string SizeName = "m_Size";
	}
}
