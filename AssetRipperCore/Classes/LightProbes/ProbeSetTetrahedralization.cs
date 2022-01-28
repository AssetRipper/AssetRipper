using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.LightProbes
{
	public sealed class ProbeSetTetrahedralization : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Tetrahedra = reader.ReadAssetArray<Tetrahedron>();
			HullRays = reader.ReadAssetArray<Vector3f>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TetrahedraName, Tetrahedra.ExportYAML(container));
			node.Add(HullRaysName, HullRays.ExportYAML(container));
			return node;
		}

		public Tetrahedron[] Tetrahedra { get; set; }
		public Vector3f[] HullRays { get; set; }

		public const string TetrahedraName = "m_Tetrahedra";
		public const string HullRaysName = "m_HullRays";
	}
}
