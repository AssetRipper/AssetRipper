using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.NavMeshData
{
	public sealed class HeightMeshBVNode : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Min.Read(reader);
			Max.Read(reader);
			I = reader.ReadInt32();
			N = reader.ReadInt32();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(MinName, Min.ExportYaml(container));
			node.Add(MaxName, Max.ExportYaml(container));
			node.Add(IName, I);
			node.Add(NName, N);
			return node;
		}

		public int I { get; set; }
		public int N { get; set; }

		public const string MinName = "min";
		public const string MaxName = "max";
		public const string IName = "i";
		public const string NName = "n";

		public Vector3f Min = new();
		public Vector3f Max = new();
	}
}
