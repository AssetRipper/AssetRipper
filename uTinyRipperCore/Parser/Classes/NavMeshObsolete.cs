using System.Collections.Generic;
using System.IO;
using uTinyRipper.Project;
using uTinyRipper.Classes.NavMeshDatas;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

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

			m_meshData = reader.ReadByteArray();
			m_heightmaps = reader.ReadAssetArray<HeightmapData>();
		}

		public override IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			foreach (Object dependency in base.FetchDependencies(context))
			{
				yield return dependency;
			}

			foreach (Object asset in context.FetchDependencies(Heightmaps, HeightmapsName))
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

		public IReadOnlyList<byte> MeshData => m_meshData;
		public IReadOnlyList<HeightmapData> Heightmaps => m_heightmaps;

		public const string MeshDataName = "m_MeshData";
		public const string HeightmapsName = "m_Heightmaps";

		private byte[] m_meshData;
		private HeightmapData[] m_heightmaps;
	}
}
