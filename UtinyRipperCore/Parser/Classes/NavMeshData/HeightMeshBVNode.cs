using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.NavMeshDatas
{
	public struct HeightMeshBVNode : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Min.Read(stream);
			Max.Read(stream);
			I = stream.ReadInt32();
			N = stream.ReadInt32();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("min", Min.ExportYAML(exporter));
			node.Add("max", Max.ExportYAML(exporter));
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
