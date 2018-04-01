using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.TerrainDatas
{
	public struct SplatDatabase : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.0.1 and greater
		/// </summary>
		public static bool IsReadColorSpace(Version version)
		{
			return version.IsGreaterEqual(5, 0, 1);
		}

		public void Read(AssetStream stream)
		{
			m_splats = stream.ReadArray<SplatPrototype>();
			m_alphaTextures = stream.ReadArray<PPtr<Texture2D>>();
			AlphamapResolution = stream.ReadInt32();
			BaseMapResolution = stream.ReadInt32();
			if (IsReadColorSpace(stream.Version))
			{
				ColorSpace = stream.ReadInt32();
				MaterialRequiresMetallic = stream.ReadBoolean();
				MaterialRequiresSmoothness = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Splats", Splats.ExportYAML(exporter));
			node.Add("m_AlphaTextures", AlphaTextures.ExportYAML(exporter));
			node.Add("m_AlphamapResolution", AlphamapResolution);
			node.Add("m_BaseMapResolution", BaseMapResolution);
			node.Add("m_ColorSpace", ColorSpace);
			node.Add("m_MaterialRequiresMetallic", MaterialRequiresMetallic);
			node.Add("m_MaterialRequiresSmoothness", MaterialRequiresSmoothness);
			return node;
		}

		public IReadOnlyList<SplatPrototype> Splats => m_splats;
		public IReadOnlyList<PPtr<Texture2D>> AlphaTextures => m_alphaTextures;
		public int AlphamapResolution { get; private set; }
		public int BaseMapResolution { get; private set; }
		public int ColorSpace { get; private set; }
		public bool MaterialRequiresMetallic { get; private set; }
		public bool MaterialRequiresSmoothness { get; private set; }

		private SplatPrototype[] m_splats;
		private PPtr<Texture2D>[] m_alphaTextures;
	}
}
