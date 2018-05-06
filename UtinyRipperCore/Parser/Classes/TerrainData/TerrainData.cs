using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.TerrainDatas;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			SplatDatabase.Read(stream);
			DetailDatabase.Read(stream);
			Heightmap.Read(stream);
			if (IsReadLightmap(stream.Version))
			{
				Lightmap.Read(stream);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			
			foreach(Object @object in SplatDatabase.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			foreach(Object @object in DetailDatabase.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			foreach(Object @object in Heightmap.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			
			if (IsReadLightmap(file.Version))
			{
				yield return Lightmap.FetchDependency(file, isLog, ToLogString, "m_Lightmap");
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_SplatDatabase", SplatDatabase.ExportYAML(container));
			node.Add("m_DetailDatabase", DetailDatabase.ExportYAML(container));
			node.Add("m_Heightmap", Heightmap.ExportYAML(container));
			return node;
		}

		public SplatDatabase SplatDatabase;
		public DetailDatabase DetailDatabase;
		public Heightmap Heightmap;
		public PPtr<Texture2D> Lightmap;
	}
}
