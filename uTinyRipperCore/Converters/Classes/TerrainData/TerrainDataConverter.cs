using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;

namespace uTinyRipper.Converters
{
	public static class TerrainDataConverter
	{
		public static TerrainData Convert(IExportContainer container, TerrainData origin)
		{
			TerrainData instance = new TerrainData(origin.AssetInfo);
			NamedObjectConverter.Convert(container, origin, instance);
			instance.SplatDatabase = origin.SplatDatabase.Convert(container);
			instance.DetailDatabase = origin.DetailDatabase.Convert(container);
			instance.Heightmap = origin.Heightmap.Convert(container);
			if (TerrainData.HasLightmap(container.ExportVersion))
			{
				instance.Lightmap = origin.Lightmap;
			}
			return instance;
		}
	}
}
