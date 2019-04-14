using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct EnlightenSceneMapping : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadProbesets(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadTerrainChunks(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2017);
		}

		public void Read(AssetReader reader)
		{
			m_renderers = reader.ReadAssetArray<EnlightenRendererInformation>();
			if(IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			m_systems = reader.ReadAssetArray<EnlightenSystemInformation>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadProbesets(reader.Version))
			{
				m_probesets = reader.ReadAssetArray<Hash128>();
				reader.AlignStream(AlignType.Align4);
			}
			m_systemAtlases = reader.ReadAssetArray<EnlightenSystemAtlasInformation>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			if(IsReadTerrainChunks(reader.Version))
			{
				m_terrainChunks = reader.ReadAssetArray<EnlightenTerrainChunksInformation>();
				if (IsAlign(reader.Version))
				{
					reader.AlignStream(AlignType.Align4);
				}
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(EnlightenRendererInformation renderer in Renderers)
			{
				foreach (Object dep in renderer.FetchDependencies(file, isLog))
				{
					yield return dep;
				}
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Renderers", Renderers.ExportYAML(container));
			node.Add("m_Systems", Systems.ExportYAML(container));
			node.Add("m_Probesets", Probesets.ExportYAML(container));
			node.Add("m_SystemAtlases", SystemAtlases.ExportYAML(container));
			node.Add("m_TerrainChunks", TerrainChunks.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<EnlightenRendererInformation> Renderers => m_renderers;
		public IReadOnlyList<EnlightenSystemInformation> Systems => m_systems;
		public IReadOnlyList<Hash128> Probesets => m_probesets;
		public IReadOnlyList<EnlightenSystemAtlasInformation> SystemAtlases => m_systemAtlases;
		public IReadOnlyList<EnlightenTerrainChunksInformation> TerrainChunks => m_terrainChunks;

		private EnlightenRendererInformation[] m_renderers;
		private EnlightenSystemInformation[] m_systems;
		private Hash128[] m_probesets;
		private EnlightenSystemAtlasInformation[] m_systemAtlases;
		private EnlightenTerrainChunksInformation[] m_terrainChunks;
	}
}
