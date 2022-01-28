using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.NavMeshData
{
	public sealed class NavMeshBuildDebugSettings : IAssetReadable, IYAMLExportable
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
