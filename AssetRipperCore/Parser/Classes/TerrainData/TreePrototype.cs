using AssetRipper.Converters.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.Parser.IO.Asset.Writer;
using AssetRipper.YAML;
using System.Collections.Generic;

namespace AssetRipper.Parser.Classes.TerrainData
{
	public struct TreePrototype : IAsset, IDependent
	{
		public void Read(AssetReader reader)
		{
			Prefab.Read(reader);
			BendFactor = reader.ReadSingle();
		}

		public void Write(AssetWriter writer)
		{
			Prefab.Write(writer);
			writer.Write(BendFactor);
		}

		public IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Prefab, PrefabName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(PrefabName, Prefab.ExportYAML(container));
			node.Add(BendFactorName, BendFactor);
			return node;
		}

		public float BendFactor { get; set; }

		public const string PrefabName = "prefab";
		public const string BendFactorName = "bendFactor";

		public PPtr<GameObject.GameObject> Prefab;
	}
}
