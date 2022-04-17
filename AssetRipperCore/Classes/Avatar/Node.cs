using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class Node : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			ParentId = reader.ReadInt32();
			AxesId = reader.ReadInt32();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(ParentIdName, ParentId);
			node.Add(AxesIdName, AxesId);
			return node;
		}

		public override string ToString()
		{
			return $"P:{ParentId} A:{AxesId}";
		}

		public int ParentId { get; set; }
		public int AxesId { get; set; }

		public const string ParentIdName = "m_ParentId";
		public const string AxesIdName = "m_AxesId";
	}
}
