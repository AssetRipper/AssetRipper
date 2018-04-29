using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.LightmapSettingss
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
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadTerrainChunks(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2017);
		}

		public void Read(AssetStream stream)
		{
			m_renderers = stream.ReadArray<EnlightenRendererInformation>();
			if(IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
			m_systems = stream.ReadArray<EnlightenSystemInformation>();
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
			if (IsReadProbesets(stream.Version))
			{
				m_probesets = stream.ReadArray<Hash128>();
				stream.AlignStream(AlignType.Align4);
			}
			m_systemAtlases = stream.ReadArray<EnlightenSystemAtlasInformation>();
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
			if(IsReadTerrainChunks(stream.Version))
			{
				m_terrainChunks = stream.ReadArray<EnlightenTerrainChunksInformation>();
				if (IsAlign(stream.Version))
				{
					stream.AlignStream(AlignType.Align4);
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

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Renderers", Renderers.ExportYAML(exporter));
			node.Add("m_Systems", Systems.ExportYAML(exporter));
			node.Add("m_Probesets", Probesets.ExportYAML(exporter));
			node.Add("m_SystemAtlases", SystemAtlases.ExportYAML(exporter));
			node.Add("m_TerrainChunks", TerrainChunks.ExportYAML(exporter));
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
