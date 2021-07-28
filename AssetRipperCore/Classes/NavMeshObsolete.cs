using AssetRipper.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Misc;
using AssetRipper.Classes.NavMeshData;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;
using AssetRipper.YAML.Extensions;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Classes
{
	/// <summary>
	/// NavMesh previously
	/// </summary>
	public sealed class NavMeshObsolete : NamedObject
	{
		public NavMeshObsolete(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			MeshData = reader.ReadByteArray();
			Heightmaps = reader.ReadAssetArray<HeightmapData>();
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object.Object> asset in context.FetchDependencies(Heightmaps, HeightmapsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(MeshDataName, MeshData.ExportYAML());
			node.Add(HeightmapsName, Heightmaps.ExportYAML(container));
			return node;
		}

		public override string ExportPath => Path.Combine(AssetsKeyword, OcclusionCullingSettings.OcclusionCullingSettings.SceneKeyword, ClassID.ToString());

		public byte[] MeshData { get; set; }
		public HeightmapData[] Heightmaps { get; set; }

		public const string MeshDataName = "m_MeshData";
		public const string HeightmapsName = "m_Heightmaps";
	}
}
