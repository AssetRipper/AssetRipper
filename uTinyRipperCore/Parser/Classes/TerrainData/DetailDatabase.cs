using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
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
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(3))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			m_patches = reader.ReadArray<DetailPatch>();
			m_detailPrototypes = reader.ReadArray<DetailPrototype>();
			PatchCount = reader.ReadInt32();
			PatchSamples = reader.ReadInt32();
			m_randomRotations = reader.ReadArray<Vector3f>();
			if (IsReadAtlasTexture(reader.Version))
			{
				AtlasTexture.Read(reader);
			}
			WavingGrassTint.Read(reader);
			WavingGrassStrength = reader.ReadSingle();
			WavingGrassAmount = reader.ReadSingle();
			WavingGrassSpeed = reader.ReadSingle();
			m_treeInstances = reader.ReadArray<TreeInstance>();
			m_treePrototypes = reader.ReadArray<TreePrototype>();
			if (IsReadPreloadTextureAtlasData(reader.Version))
			{
				m_preloadTextureAtlasData = reader.ReadArray<PPtr<Texture2D>>();
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (DetailPrototype prototype in DetailPrototypes)
			{
				foreach(Object @object in prototype.FetchDependencies(file, isLog))
				{
					yield return @object;
				}
			}
			
			if (IsReadAtlasTexture(file.Version))
			{
				yield return AtlasTexture.FetchDependency(file, isLog, () => nameof(DetailDatabase), "m_AtlasTexture");
			}

			foreach (TreePrototype prototype in TreePrototypes)
			{
				foreach(Object @object in prototype.FetchDependencies(file, isLog))
				{
					yield return @object;
				}
			}

			foreach (PPtr<Texture2D> preloadTexture in PreloadTextureAtlasData)
			{
				yield return preloadTexture.FetchDependency(file, isLog, () => nameof(DetailDatabase), "m_PreloadTextureAtlasData");
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Patches", Patches.ExportYAML(container));
			node.Add("m_DetailPrototypes", DetailPrototypes.ExportYAML(container));
			node.Add("m_PatchCount", PatchCount);
			node.Add("m_PatchSamples", PatchSamples);
			node.Add("m_RandomRotations", RandomRotations.ExportYAML(container));
			node.Add("WavingGrassTint", WavingGrassTint.ExportYAML(container));
			node.Add("m_WavingGrassStrength", WavingGrassStrength);
			node.Add("m_WavingGrassAmount", WavingGrassAmount);
			node.Add("m_WavingGrassSpeed", WavingGrassSpeed);
			node.Add("m_TreeInstances", TreeInstances.ExportYAML(container));
			node.Add("m_TreePrototypes", TreePrototypes.ExportYAML(container));
			node.Add("m_PreloadTextureAtlasData", PreloadTextureAtlasData.ExportYAML(container));
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
