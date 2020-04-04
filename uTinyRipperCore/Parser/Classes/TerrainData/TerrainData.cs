using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes.TerrainDatas;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class TerrainData : NamedObject
	{
		public TerrainData(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasLightmap(Version version) => version.IsLess(3);
		/// <summary>
		/// (2018.4.14 to 2019.1 exclusive) or (2019.2.17 and greater)
		/// </summary>
		public static bool HasPreloadShaders(Version version)
		{
			if (version.IsGreaterEqual(2019, 2, 17))
			{
				return true;
			}
			if (version.IsGreaterEqual(2018, 4, 14))
			{
				return version.IsLess(2019);
			}
			return false;
		}

		public override Object Convert(IExportContainer container)
		{
			return TerrainDataConverter.Convert(container, this);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			SplatDatabase.Read(reader);
			DetailDatabase.Read(reader);
			Heightmap.Read(reader);
			if (HasLightmap(reader.Version))
			{
				Lightmap.Read(reader);
			}
			if (HasPreloadShaders(reader.Version))
			{
				PreloadShaders = reader.ReadAssetArray<PPtr<Shader>>();
			}
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			SplatDatabase.Write(writer);
			DetailDatabase.Write(writer);
			Heightmap.Write(writer);
			if (HasLightmap(writer.Version))
			{
				Lightmap.Write(writer);
			}
			if (HasPreloadShaders(writer.Version))
			{
				PreloadShaders.Write(writer);
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object> asset in context.FetchDependencies(SplatDatabase, SplatDatabaseName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(DetailDatabase, DetailDatabaseName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(Heightmap, HeightmapName))
			{
				yield return asset;
			}

			if (HasLightmap(context.Version))
			{
				yield return context.FetchDependency(Lightmap, LightmapName);
			}
			if (HasPreloadShaders(context.Version))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(PreloadShaders, PreloadShadersName))
				{
					yield return asset;
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(SplatDatabaseName, SplatDatabase.ExportYAML(container));
			node.Add(DetailDatabaseName, DetailDatabase.ExportYAML(container));
			node.Add(HeightmapName, Heightmap.ExportYAML(container));
			if (HasLightmap(container.ExportVersion))
			{
				node.Add(LightmapName, Lightmap.ExportYAML(container));
			}
			if (HasPreloadShaders(container.ExportVersion))
			{
				node.Add(PreloadShadersName, PreloadShaders.ExportYAML(container));
			}
			return node;
		}

		public override string ExportPath => Path.Combine(AssetsKeyword, nameof(Terrain), nameof(TerrainData));

		public PPtr<Shader>[] PreloadShaders { get; set; }

		public const string SplatDatabaseName = "m_SplatDatabase";
		public const string DetailDatabaseName = "m_DetailDatabase";
		public const string HeightmapName = "m_Heightmap";
		public const string LightmapName = "m_Lightmap";
		public const string PreloadShadersName = "m_PreloadShaders";

		public SplatDatabase SplatDatabase;
		public DetailDatabase DetailDatabase;
		public Heightmap Heightmap;
		public PPtr<Texture2D> Lightmap;
	}
}
