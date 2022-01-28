using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.AudioClip
{
	public sealed class StreamedResource : IStreamedResource
	{
		public void Read(AssetReader reader)
		{
			Source = reader.ReadString();
			Offset = reader.ReadUInt64();
			if (HasSize(reader.Version))
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
			if (HasSize(writer.Version))
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

		/// <summary>
		/// 5.0.0f1 and greater (unknown version)
		/// </summary>
		public static bool HasSize(UnityVersion version)
		{
			return version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		}

		public const string SourceName = "m_Source";
		public const string OffsetName = "m_Offset";
		public const string SizeName = "m_Size";
	}
}
