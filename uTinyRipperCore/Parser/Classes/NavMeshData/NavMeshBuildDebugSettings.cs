using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.NavMeshDatas
{
	public struct NavMeshBuildDebugSettings : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Flags = reader.ReadByte();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(FlagsName, Flags);
			return node;
		}

		public const string FlagsName = "m_Flags";

		public byte Flags { get; private set; }
	}
}
