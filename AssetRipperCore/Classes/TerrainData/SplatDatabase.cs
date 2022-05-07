using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Converters.TerrainData;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.TerrainData
{
	public sealed class SplatDatabase : IAsset, IDependent
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			// SplatPrototype is replaced by TerrainLayer
			if (version.IsGreaterEqual(2018, 3))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasTerrainLayers(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 5.0.0p1 to 2018.3 exclusive
		/// </summary>
		public static bool HasColorSpace(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Patch) && version.IsLess(2018, 3);

		public SplatDatabase Convert(IExportContainer container)
		{
			return SplatDatabaseConverter.Convert(container, this);
		}

		public void Read(AssetReader reader)
		{
			if (HasTerrainLayers(reader.Version))
			{
				TerrainLayers = reader.ReadAssetArray<PPtr<TerrainLayer>>();
			}
			else
			{
				Splats = reader.ReadAssetArray<SplatPrototype>();
			}

			AlphaTextures = reader.ReadAssetArray<PPtr<Texture2D.Texture2D>>();
			AlphamapResolution = reader.ReadInt32();
			BaseMapResolution = reader.ReadInt32();
			if (HasColorSpace(reader.Version))
			{
				ColorSpace = reader.ReadInt32();
				MaterialRequiresMetallic = reader.ReadBoolean();
				MaterialRequiresSmoothness = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public void Write(AssetWriter writer)
		{
			if (HasTerrainLayers(writer.Version))
			{
				TerrainLayers.Write(writer);
			}
			else
			{
				Splats.Write(writer);
			}

			AlphaTextures.Write(writer);
			writer.Write(AlphamapResolution);
			writer.Write(BaseMapResolution);
			if (HasColorSpace(writer.Version))
			{
				writer.Write(ColorSpace);
				writer.Write(MaterialRequiresMetallic);
				writer.Write(MaterialRequiresSmoothness);
				writer.AlignStream();
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			if (HasTerrainLayers(container.ExportVersion))
			{
				node.Add(TerrainLayersName, TerrainLayers.ExportYaml(container));
			}
			else
			{
				node.Add(SplatsName, Splats.ExportYaml(container));
			}

			node.Add(AlphaTexturesName, AlphaTextures.ExportYaml(container));
			node.Add(AlphamapResolutionName, AlphamapResolution);
			node.Add(BaseMapResolutionName, BaseMapResolution);
			if (HasColorSpace(container.ExportVersion))
			{
				node.Add(ColorSpaceName, ColorSpace);
				node.Add(MaterialRequiresMetallicName, MaterialRequiresMetallic);
				node.Add(MaterialRequiresSmoothnessName, MaterialRequiresSmoothness);
			}
			return node;
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			if (HasTerrainLayers(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(TerrainLayers, TerrainLayersName))
				{
					yield return asset;
				}
			}
			else
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(Splats, SplatsName))
				{
					yield return asset;
				}
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(AlphaTextures, AlphaTexturesName))
			{
				yield return asset;
			}
		}

		public SplatPrototype[] Splats { get; set; }
		public PPtr<TerrainLayer>[] TerrainLayers { get; set; }
		public PPtr<Texture2D.Texture2D>[] AlphaTextures { get; set; }
		public int AlphamapResolution { get; set; }
		public int BaseMapResolution { get; set; }
		public int ColorSpace { get; set; }
		public bool MaterialRequiresMetallic { get; set; }
		public bool MaterialRequiresSmoothness { get; set; }

		public const string TerrainLayersName = "m_TerrainLayers";
		public const string SplatsName = "m_Splats";
		public const string AlphaTexturesName = "m_AlphaTextures";
		public const string AlphamapResolutionName = "m_AlphamapResolution";
		public const string BaseMapResolutionName = "m_BaseMapResolution";
		public const string ColorSpaceName = "m_ColorSpace";
		public const string MaterialRequiresMetallicName = "m_MaterialRequiresMetallic";
		public const string MaterialRequiresSmoothnessName = "m_MaterialRequiresSmoothness";
	}
}
