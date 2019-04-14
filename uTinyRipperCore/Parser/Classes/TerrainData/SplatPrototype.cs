using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct SplatPrototype : IAssetReadable, IYAMLExportable, IDependent
	{
		public SplatPrototype(bool _):
			this()
		{
			TileSize = Vector2f.One;
		}

		public SplatPrototype(TerrainLayer layer)
		{
			Texture = layer.DiffuseTexture;
			NormalMap = layer.NormalMapTexture;
			TileSize = layer.TileSize;
			TileOffset = layer.TileOffset;
			SpecularMetallic = new Vector4f(layer.Specular.R, layer.Specular.G, layer.Specular.B, layer.Metallic);
			Smoothness = layer.Smoothness;
		}

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
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadSpecularMetallic(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.0.1 and greater
		/// </summary>
		public static bool IsReadSmoothness(Version version)
		{
			return version.IsGreaterEqual(5, 0, 1);
		}

		public void Read(AssetReader reader)
		{
			Texture.Read(reader);
			if (IsReadNormalMap(reader.Version))
			{
				NormalMap.Read(reader);
			}
			TileSize.Read(reader);
			if (IsReadTileOffset(reader.Version))
			{
				TileOffset.Read(reader);
			}
			if (IsReadSpecularMetallic(reader.Version))
			{
				SpecularMetallic.Read(reader);
			}
			if (IsReadSmoothness(reader.Version))
			{
				Smoothness = reader.ReadSingle();
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Texture.FetchDependency(file, isLog, () => nameof(SplatPrototype), "texture");
			if (IsReadNormalMap(file.Version))
			{
				yield return NormalMap.FetchDependency(file, isLog, () => nameof(SplatPrototype), "normalMap");
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TextureName, Texture.ExportYAML(container));
			node.Add(NormalMapName, NormalMap.ExportYAML(container));
			node.Add(TileSizeName, TileSize.ExportYAML(container));
			node.Add(TileOffsetName, TileOffset.ExportYAML(container));
			node.Add(SpecularMetallicName, SpecularMetallic.ExportYAML(container));
			node.Add(SmoothnessName, Smoothness);
			return node;
		}

		public float Smoothness { get; private set; }

		public const string TextureName = "texture";
		public const string NormalMapName = "normalMap";
		public const string TileSizeName = "tileSize";
		public const string TileOffsetName = "tileOffset";
		public const string SpecularMetallicName = "specularMetallic";
		public const string SmoothnessName = "smoothness";

		public PPtr<Texture2D> Texture;
		public PPtr<Texture2D> NormalMap;
		public Vector2f TileSize;
		public Vector2f TileOffset;
		public Vector4f SpecularMetallic;
	}
}
