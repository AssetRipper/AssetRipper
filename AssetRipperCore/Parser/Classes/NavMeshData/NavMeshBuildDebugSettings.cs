using AssetRipper.Converters.Project;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.YAML;

namespace AssetRipper.Parser.Classes.NavMeshData
{
	public struct NavMeshBuildDebugSettings : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Flags = reader.ReadByte();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(FlagsName, Flags);
			return node;
		}

		public const string FlagsName = "m_Flags";

		public byte Flags { get; set; }
	}
}
