using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.TerrainData
{
	public sealed class TreePrototype : IAsset, IDependent
	{

		/// <summary>
		/// 2020.2 and greater
		/// </summary>
		public static bool HasNavMeshLod(UnityVersion version) => version.IsGreaterEqual(2020, 2);

		public void Read(AssetReader reader)
		{
			Prefab.Read(reader);
			BendFactor = reader.ReadSingle();
			if (HasNavMeshLod(reader.Version))
			{
				NavMeshLod = reader.ReadInt32();
			}
		}

		public void Write(AssetWriter writer)
		{
			Prefab.Write(writer);
			writer.Write(BendFactor);
			if (HasNavMeshLod(writer.Version))
			{
				writer.Write(NavMeshLod);
			}
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Prefab, PrefabName);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(PrefabName, Prefab.ExportYaml(container));
			node.Add(BendFactorName, BendFactor);
			if (HasNavMeshLod(container.ExportVersion))
			{
				node.Add(NavMeshLodName, NavMeshLod);
			}
			return node;
		}

		public float BendFactor { get; set; }
		public float NavMeshLod { get; set; }

		public const string PrefabName = "prefab";
		public const string BendFactorName = "bendFactor";
		public const string NavMeshLodName = "navMeshLod";

		public PPtr<GameObject.GameObject> Prefab = new();
	}
}
