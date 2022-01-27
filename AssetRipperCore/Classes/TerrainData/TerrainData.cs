using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Converters.TerrainData;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Classes.TerrainData
{
	public sealed class TerrainData : NamedObject, ITerrainData
	{
		public TerrainData(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasLightmap(UnityVersion version) => version.IsLess(3);
		/// <summary>
		/// (2018.4.14 to 2019.1 exclusive) or (2019.2.17 and greater)
		/// </summary>
		public static bool HasPreloadShaders(UnityVersion version)
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

		public override IUnityObjectBase ConvertLegacy(IExportContainer container)
		{
			return TerrainDataConverter.Convert(container, this);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			SplatDatabase.Read(reader);
			DetailDatabase.Read(reader);
			m_Heightmap.Read(reader);
			if (HasLightmap(reader.Version))
			{
				Lightmap.Read(reader);
			}
			if (HasPreloadShaders(reader.Version))
			{
				PreloadShaders = reader.ReadAssetArray<PPtr<Shader.Shader>>();
			}
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			SplatDatabase.Write(writer);
			DetailDatabase.Write(writer);
			m_Heightmap.Write(writer);
			if (HasLightmap(writer.Version))
			{
				Lightmap.Write(writer);
			}
			if (HasPreloadShaders(writer.Version))
			{
				PreloadShaders.Write(writer);
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(SplatDatabase, SplatDatabaseName))
			{
				yield return asset;
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(DetailDatabase, DetailDatabaseName))
			{
				yield return asset;
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(m_Heightmap, HeightmapName))
			{
				yield return asset;
			}

			if (HasLightmap(context.Version))
			{
				yield return context.FetchDependency(Lightmap, LightmapName);
			}
			if (HasPreloadShaders(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(PreloadShaders, PreloadShadersName))
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
			node.Add(HeightmapName, m_Heightmap.ExportYAML(container));
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

		public override string ExportPath => Path.Combine(AssetsKeyword, nameof(Terrain.Terrain), nameof(TerrainData));

		public PPtr<Shader.Shader>[] PreloadShaders { get; set; }

		public IHeightmap Heightmap => m_Heightmap;

		public const string SplatDatabaseName = "m_SplatDatabase";
		public const string DetailDatabaseName = "m_DetailDatabase";
		public const string HeightmapName = "m_Heightmap";
		public const string LightmapName = "m_Lightmap";
		public const string PreloadShadersName = "m_PreloadShaders";

		public SplatDatabase SplatDatabase = new();
		public DetailDatabase DetailDatabase = new();
		public Heightmap m_Heightmap = new();
		public PPtr<Texture2D.Texture2D> Lightmap = new();
	}
}
