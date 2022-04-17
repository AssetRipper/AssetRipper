using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.NavMeshData
{
	public sealed class NavMeshBuildDebugSettings : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Flags = reader.ReadByte();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(FlagsName, Flags);
			return node;
		}

		public const string FlagsName = "m_Flags";

		public byte Flags { get; set; }
	}
}
