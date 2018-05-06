using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.TerrainDatas
{
	public struct SplatPrototype : IAssetReadable, IYAMLExportable, IDependent
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
			node.Add("texture", Texture.ExportYAML(container));
			node.Add("normalMap", NormalMap.ExportYAML(container));
			node.Add("tileSize", TileSize.ExportYAML(container));
			node.Add("tileOffset", TileOffset.ExportYAML(container));
			node.Add("specularMetallic", SpecularMetallic.ExportYAML(container));
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
