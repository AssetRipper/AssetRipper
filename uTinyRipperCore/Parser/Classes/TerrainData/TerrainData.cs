using System.Collections.Generic;
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
			return node;
		}

		public const string SplatDatabaseName = "m_SplatDatabase";
		public const string DetailDatabaseName = "m_DetailDatabase";
		public const string HeightmapName = "m_Heightmap";
		public const string LightmapName = "m_Lightmap";

		public SplatDatabase SplatDatabase;
		public DetailDatabase DetailDatabase;
		public Heightmap Heightmap;
		public PPtr<Texture2D> Lightmap;
	}
}
