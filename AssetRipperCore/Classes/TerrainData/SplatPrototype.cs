using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.TerrainData
{
	public sealed class SplatPrototype : IAsset, IDependent
	{
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasNormalMap(UnityVersion version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasTileOffset(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 5.0.0f1 and greater (unknown version)
		/// </summary>
		public static bool HasSpecularMetallic(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 5.0.0p1 and greater
		/// </summary>
		public static bool HasSmoothness(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Patch);

		public void Read(AssetReader reader)
		{
			Texture.Read(reader);
			if (HasNormalMap(reader.Version))
			{
				NormalMap.Read(reader);
			}
			TileSize.Read(reader);
			if (HasTileOffset(reader.Version))
			{
				TileOffset.Read(reader);
			}
			if (HasSpecularMetallic(reader.Version))
			{
				SpecularMetallic.Read(reader);
			}
			if (HasSmoothness(reader.Version))
			{
				Smoothness = reader.ReadSingle();
			}
		}

		public void Write(AssetWriter writer)
		{
			Texture.Write(writer);
			if (HasNormalMap(writer.Version))
			{
				NormalMap.Write(writer);
			}
			TileSize.Write(writer);
			if (HasTileOffset(writer.Version))
			{
				TileOffset.Write(writer);
			}
			if (HasSpecularMetallic(writer.Version))
			{
				SpecularMetallic.Write(writer);
			}
			if (HasSmoothness(writer.Version))
			{
				writer.Write(Smoothness);
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(TextureName, Texture.ExportYaml(container));
			if (HasNormalMap(container.ExportVersion))
			{
				node.Add(NormalMapName, NormalMap.ExportYaml(container));
			}
			node.Add(TileSizeName, TileSize.ExportYaml(container));
			if (HasTileOffset(container.ExportVersion))
			{
				node.Add(TileOffsetName, TileOffset.ExportYaml(container));
			}
			if (HasSpecularMetallic(container.ExportVersion))
			{
				node.Add(SpecularMetallicName, SpecularMetallic.ExportYaml(container));
			}
			if (HasSmoothness(container.ExportVersion))
			{
				node.Add(SmoothnessName, Smoothness);
			}
			return node;
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Texture, TextureName);
			if (HasNormalMap(context.Version))
			{
				yield return context.FetchDependency(NormalMap, NormalMapName);
			}
		}

		public float Smoothness { get; set; }

		public const string TextureName = "texture";
		public const string NormalMapName = "normalMap";
		public const string TileSizeName = "tileSize";
		public const string TileOffsetName = "tileOffset";
		public const string SpecularMetallicName = "specularMetallic";
		public const string SmoothnessName = "smoothness";

		public PPtr<Texture2D.Texture2D> Texture = new();
		public PPtr<Texture2D.Texture2D> NormalMap = new();
		public Vector2f TileSize = new();
		public Vector2f TileOffset = new();
		public Vector4f SpecularMetallic = new();
	}
}
