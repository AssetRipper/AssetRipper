using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.TerrainDatas
{
	public struct TreePrototype : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetStream stream)
		{
			Prefab.Read(stream);
			BendFactor = stream.ReadSingle();
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Prefab.FetchDependency(file, isLog, () => nameof(TreePrototype), "prefab");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("prefab", Prefab.ExportYAML(container));
			node.Add("bendFactor", BendFactor);
			return node;
		}

		public float BendFactor { get; private set; }

		public PPtr<GameObject> Prefab;
	}
}
