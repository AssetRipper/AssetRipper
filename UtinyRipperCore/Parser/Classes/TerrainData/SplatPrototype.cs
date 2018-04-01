using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.TerrainDatas
{
	public struct SplatPrototype : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadNormalMap(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadTileOffset(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadSpecularMetallic(Version version)
		{
#warning unknown
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 5.0.1 and greater
		/// </summary>
		public static bool IsReadSmoothness(Version version)
		{
			return version.IsGreaterEqual(5, 0, 1);
		}

		public void Read(AssetStream stream)
		{
			Texture.Read(stream);
			if (IsReadNormalMap(stream.Version))
			{
				NormalMap.Read(stream);
			}
			TileSize.Read(stream);
			if (IsReadTileOffset(stream.Version))
			{
				TileOffset.Read(stream);
			}
			if (IsReadSpecularMetallic(stream.Version))
			{
				SpecularMetallic.Read(stream);
			}
			if (IsReadSmoothness(stream.Version))
			{
				Smoothness = stream.ReadSingle();
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("texture", Texture.ExportYAML(exporter));
			node.Add("normalMap", NormalMap.ExportYAML(exporter));
			node.Add("tileSize", TileSize.ExportYAML(exporter));
			node.Add("tileOffset", TileOffset.ExportYAML(exporter));
			node.Add("specularMetallic", SpecularMetallic.ExportYAML(exporter));
			node.Add("smoothness", Smoothness);
			return node;
		}

		public float Smoothness { get; private set; }

		public PPtr<Texture2D> Texture;
		public PPtr<Texture2D> NormalMap;
		public Vector2f TileSize;
		public Vector2f TileOffset;
		public Vector4f SpecularMetallic;
	}
}
