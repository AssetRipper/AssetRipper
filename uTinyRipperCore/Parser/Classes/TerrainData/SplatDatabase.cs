using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct SplatDatabase : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadTerrainLayers(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 5.0.1 to 2018.3 exclusive
		/// </summary>
		public static bool IsReadColorSpace(Version version)
		{
			return version.IsGreaterEqual(5, 0, 1) && version.IsLess(2018, 3);
		}

		private static int GetSerializedVersion(Version version)
		{
			// SplatPrototype is replaced with TerrainLayer
			if (version.IsGreaterEqual(2018, 3))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			if (IsReadTerrainLayers(reader.Version))
			{
				m_terrainLayers = reader.ReadAssetArray<PPtr<TerrainLayer>>();
			}
			else
			{
				m_splats = reader.ReadAssetArray<SplatPrototype>();
			}
			m_alphaTextures = reader.ReadAssetArray<PPtr<Texture2D>>();
			AlphamapResolution = reader.ReadInt32();
			BaseMapResolution = reader.ReadInt32();
			if (IsReadColorSpace(reader.Version))
			{
				ColorSpace = reader.ReadInt32();
				MaterialRequiresMetallic = reader.ReadBoolean();
				MaterialRequiresSmoothness = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (SplatPrototype prototype in Splats)
			{
				foreach (Object asset in prototype.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}

			foreach (PPtr<Texture2D> alphaTexture in AlphaTextures)
			{
				yield return alphaTexture.FetchDependency(file, isLog, () => nameof(SplatDatabase), "m_AlphaTextures");
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			if (IsReadTerrainLayers(container.ExportVersion))
			{
				node.Add(TerrainLayersName, TerrainLayers.ExportYAML(container));
			}
			else
			{
				node.Add(SplatsName, GetSplats(container).ExportYAML(container));
			}
			node.Add(AlphaTexturesName, AlphaTextures.ExportYAML(container));
			node.Add(AlphamapResolutionName, AlphamapResolution);
			node.Add(BaseMapResolutionName, BaseMapResolution);
			if (IsReadColorSpace(container.ExportVersion))
			{
				node.Add(ColorSpaceName, ColorSpace);
				node.Add(MaterialRequiresMetallicName, MaterialRequiresMetallic);
				node.Add(MaterialRequiresSmoothnessName, MaterialRequiresSmoothness);
			}
			return node;
		}

		private IReadOnlyList<SplatPrototype> GetSplats(IExportContainer container)
		{
			if (IsReadTerrainLayers(container.Version))
			{
				SplatPrototype[] splats = new SplatPrototype[m_terrainLayers.Length];
				for (int i = 0; i < splats.Length; i++)
				{
					TerrainLayer layer = m_terrainLayers[i].FindAsset(container);
					splats[i] = layer == null ? new SplatPrototype(true) : new SplatPrototype(layer);
				}
				return splats;
			}
			else
			{
				return Splats;
			}
		}

		public IReadOnlyList<SplatPrototype> Splats => m_splats;
		public IReadOnlyList<PPtr<TerrainLayer>> TerrainLayers => m_terrainLayers;
		public IReadOnlyList<PPtr<Texture2D>> AlphaTextures => m_alphaTextures;
		public int AlphamapResolution { get; private set; }
		public int BaseMapResolution { get; private set; }
		public int ColorSpace { get; private set; }
		public bool MaterialRequiresMetallic { get; private set; }
		public bool MaterialRequiresSmoothness { get; private set; }

		public const string TerrainLayersName = "m_TerrainLayers";
		public const string SplatsName = "m_Splats";
		public const string AlphaTexturesName = "m_AlphaTextures";
		public const string AlphamapResolutionName = "m_AlphamapResolution";
		public const string BaseMapResolutionName = "m_BaseMapResolution";
		public const string ColorSpaceName = "m_ColorSpace";
		public const string MaterialRequiresMetallicName = "m_MaterialRequiresMetallic";
		public const string MaterialRequiresSmoothnessName = "m_MaterialRequiresSmoothness";

		private SplatPrototype[] m_splats;
		private PPtr<TerrainLayer>[] m_terrainLayers;
		private PPtr<Texture2D>[] m_alphaTextures;
	}
}
