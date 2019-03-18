using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.NavMeshDatas
{
	public struct HeightMeshBVNode : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Min.Read(reader);
			Max.Read(reader);
			I = reader.ReadInt32();
			N = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("min", Min.ExportYAML(container));
			node.Add("max", Max.ExportYAML(container));
			node.Add("i", I);
			node.Add("n", N);
			return node;
		}

		public int I { get; private set; }
		public int N { get; private set; }

		public Vector3f Min;
		public Vector3f Max;
	}
}
