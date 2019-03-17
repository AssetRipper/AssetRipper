using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct TreePrototype : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Prefab.Read(reader);
			BendFactor = reader.ReadSingle();
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Prefab.FetchDependency(file, isLog, () => nameof(TreePrototype), "prefab");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(PrefabName, Prefab.ExportYAML(container));
			node.Add(BendFactorName, BendFactor);
			return node;
		}

		public float BendFactor { get; private set; }

		public const string PrefabName = "prefab";
		public const string BendFactorName = "bendFactor";

		public PPtr<GameObject> Prefab;
	}
}
