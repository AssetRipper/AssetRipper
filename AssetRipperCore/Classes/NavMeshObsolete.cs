using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.NavMeshData;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Classes
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

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(Heightmaps, HeightmapsName))
			{
				yield return asset;
			}
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.Add(MeshDataName, MeshData.ExportYaml());
			node.Add(HeightmapsName, Heightmaps.ExportYaml(container));
			return node;
		}

		public override string ExportPath => Path.Combine(AssetsKeyword, OcclusionCullingSettings.OcclusionCullingSettings.SceneKeyword, ClassID.ToString());

		public byte[] MeshData { get; set; }
		public HeightmapData[] Heightmaps { get; set; }

		public const string MeshDataName = "m_MeshData";
		public const string HeightmapsName = "m_Heightmaps";
	}
}
