using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.TerrainDatas;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class TerrainData : NamedObject
	{
		public TerrainData(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool IsReadLightmap(Version version)
		{
			return version.IsLess(3);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			SplatDatabase.Read(reader);
			DetailDatabase.Read(reader);
			Heightmap.Read(reader);
			if (IsReadLightmap(reader.Version))
			{
				Lightmap.Read(reader);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			foreach(Object asset in SplatDatabase.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			foreach(Object asset in DetailDatabase.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			foreach(Object asset in Heightmap.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			if (IsReadLightmap(file.Version))
			{
				yield return Lightmap.FetchDependency(file, isLog, ToLogString, LightmapName);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(SplatDatabaseName, SplatDatabase.ExportYAML(container));
			node.Add(DetailDatabaseName, DetailDatabase.ExportYAML(container));
			node.Add(HeightmapName, Heightmap.ExportYAML(container));
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
