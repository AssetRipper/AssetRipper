using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct DetailDatabase : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		public static bool IsReadAtlasTexture(Version version)
		{
			return version.IsLess(2, 6);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsReadPreloadTextureAtlasData(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(3))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			m_patches = reader.ReadAssetArray<DetailPatch>();
			m_detailPrototypes = reader.ReadAssetArray<DetailPrototype>();
			PatchCount = reader.ReadInt32();
			PatchSamples = reader.ReadInt32();
			m_randomRotations = reader.ReadAssetArray<Vector3f>();
			if (IsReadAtlasTexture(reader.Version))
			{
				AtlasTexture.Read(reader);
			}
			WavingGrassTint.Read(reader);
			WavingGrassStrength = reader.ReadSingle();
			WavingGrassAmount = reader.ReadSingle();
			WavingGrassSpeed = reader.ReadSingle();
			m_treeInstances = reader.ReadAssetArray<TreeInstance>();
			m_treePrototypes = reader.ReadAssetArray<TreePrototype>();
			if (IsReadPreloadTextureAtlasData(reader.Version))
			{
				m_preloadTextureAtlasData = reader.ReadAssetArray<PPtr<Texture2D>>();
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (DetailPrototype prototype in DetailPrototypes)
			{
				foreach(Object asset in prototype.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
			
			if (IsReadAtlasTexture(file.Version))
			{
				yield return AtlasTexture.FetchDependency(file, isLog, () => nameof(DetailDatabase), "m_AtlasTexture");
			}

			foreach (TreePrototype prototype in TreePrototypes)
			{
				foreach(Object asset in prototype.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}

			foreach (PPtr<Texture2D> preloadTexture in PreloadTextureAtlasData)
			{
				yield return preloadTexture.FetchDependency(file, isLog, () => nameof(DetailDatabase), "m_PreloadTextureAtlasData");
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(PatchesName, Patches.ExportYAML(container));
			node.Add(DetailPrototypesName, DetailPrototypes.ExportYAML(container));
			node.Add(PatchCountName, PatchCount);
			node.Add(PatchSamplesName, PatchSamples);
			node.Add(RandomRotationsName, RandomRotations.ExportYAML(container));
			node.Add(WavingGrassTintName, WavingGrassTint.ExportYAML(container));
			node.Add(WavingGrassStrengthName, WavingGrassStrength);
			node.Add(WavingGrassAmountName, WavingGrassAmount);
			node.Add(WavingGrassSpeedName, WavingGrassSpeed);
			node.Add(TreeInstancesName, TreeInstances.ExportYAML(container));
			node.Add(TreePrototypesName, TreePrototypes.ExportYAML(container));
			node.Add(PreloadTextureAtlasDataName, PreloadTextureAtlasData.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<DetailPatch> Patches => m_patches;
		public IReadOnlyList<DetailPrototype> DetailPrototypes => m_detailPrototypes;
		public int PatchCount { get; private set; }
		public int PatchSamples { get; private set; }
		public IReadOnlyList<Vector3f> RandomRotations => m_randomRotations;
		public float WavingGrassStrength { get; private set; }
		public float WavingGrassAmount { get; private set; }
		public float WavingGrassSpeed { get; private set; }
		public IReadOnlyList<TreeInstance> TreeInstances => m_treeInstances;
		public IReadOnlyList<TreePrototype> TreePrototypes => m_treePrototypes;
		public IReadOnlyList<PPtr<Texture2D>> PreloadTextureAtlasData => m_preloadTextureAtlasData;

		public const string PatchesName = "m_Patches";
		public const string DetailPrototypesName = "m_DetailPrototypes";
		public const string PatchCountName = "m_PatchCount";
		public const string PatchSamplesName = "m_PatchSamples";
		public const string RandomRotationsName = "m_RandomRotations";
		public const string WavingGrassTintName = "WavingGrassTint";
		public const string WavingGrassStrengthName = "m_WavingGrassStrength";
		public const string WavingGrassAmountName = "m_WavingGrassAmount";
		public const string WavingGrassSpeedName = "m_WavingGrassSpeed";
		public const string TreeInstancesName = "m_TreeInstances";
		public const string TreePrototypesName = "m_TreePrototypes";
		public const string PreloadTextureAtlasDataName = "m_PreloadTextureAtlasData";

		public PPtr<Texture2D> AtlasTexture;
		public ColorRGBAf WavingGrassTint;

		private DetailPatch[] m_patches;
		private DetailPrototype[] m_detailPrototypes;
		private Vector3f[] m_randomRotations;
		private TreeInstance[] m_treeInstances;
		private TreePrototype[] m_treePrototypes;
		private PPtr<Texture2D>[] m_preloadTextureAtlasData;
	}
}
