using System.Collections.Generic;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct EnlightenSceneMapping : IAsset, IDependent
	{
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasProbesets(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasTerrainChunks(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(2017);

		public void Read(AssetReader reader)
		{
			Renderers = reader.ReadAssetArray<EnlightenRendererInformation>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}
			Systems = reader.ReadAssetArray<EnlightenSystemInformation>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}
			if (HasProbesets(reader.Version))
			{
				Probesets = reader.ReadAssetArray<Hash128>();
				reader.AlignStream();
			}
			SystemAtlases = reader.ReadAssetArray<EnlightenSystemAtlasInformation>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}
			if (HasTerrainChunks(reader.Version))
			{
				TerrainChunks = reader.ReadAssetArray<EnlightenTerrainChunksInformation>();
				if (IsAlign(reader.Version))
				{
					reader.AlignStream();
				}
			}
		}

		public void Write(AssetWriter writer)
		{
			Renderers.Write(writer);
			if (IsAlign(writer.Version))
			{
				writer.AlignStream();
			}
			Systems.Write(writer);
			if (IsAlign(writer.Version))
			{
				writer.AlignStream();
			}
			if (HasProbesets(writer.Version))
			{
				Probesets.Write(writer);
				writer.AlignStream();
			}
			SystemAtlases.Write(writer);
			if (IsAlign(writer.Version))
			{
				writer.AlignStream();
			}
			if (HasTerrainChunks(writer.Version))
			{
				TerrainChunks.Write(writer);
				if (IsAlign(writer.Version))
				{
					writer.AlignStream();
				}
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in context.FetchDependencies(Renderers, RenderersName))
			{
				yield return asset;
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(RenderersName, Renderers.ExportYAML(container));
			node.Add(SystemsName, Systems.ExportYAML(container));
			node.Add(ProbesetsName, Probesets.ExportYAML(container));
			node.Add(SystemAtlasesName, SystemAtlases.ExportYAML(container));
			node.Add(TerrainChunksName, TerrainChunks.ExportYAML(container));
			return node;
		}

		public EnlightenRendererInformation[] Renderers { get; set; }
		public EnlightenSystemInformation[] Systems { get; set; }
		public Hash128[] Probesets { get; set; }
		public EnlightenSystemAtlasInformation[] SystemAtlases { get; set; }
		public EnlightenTerrainChunksInformation[] TerrainChunks { get; set; }

		public const string RenderersName = "m_Renderers";
		public const string SystemsName = "m_Systems";
		public const string ProbesetsName = "m_Probesets";
		public const string SystemAtlasesName = "m_SystemAtlases";
		public const string TerrainChunksName = "m_TerrainChunks";
	}
}
