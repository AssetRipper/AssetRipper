using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes.NavMeshDatas;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// NavMesh previously
	/// </summary>
	public sealed class NavMeshObsolete : NamedObject
	{
		public NavMeshObsolete(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			MeshData = reader.ReadByteArray();
			Heightmaps = reader.ReadAssetArray<HeightmapData>();
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object> asset in context.FetchDependencies(Heightmaps, HeightmapsName))
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

		public override string ExportPath => Path.Combine(AssetsKeyword, OcclusionCullingSettings.SceneKeyword, ClassID.ToString());

		public byte[] MeshData { get; set; }
		public HeightmapData[] Heightmaps { get; set; }

		public const string MeshDataName = "m_MeshData";
		public const string HeightmapsName = "m_Heightmaps";
	}
}
