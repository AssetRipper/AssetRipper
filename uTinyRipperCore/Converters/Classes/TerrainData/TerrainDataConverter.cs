using System;
using System.Linq;
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
			if (TerrainData.HasPreloadShaders(container.ExportVersion))
			{
				instance.PreloadShaders = GetPreloadShaders(container, origin);
			}
			return instance;
		}

		private static PPtr<Shader>[] GetPreloadShaders(IExportContainer container, TerrainData origin)
		{
			return TerrainData.HasPreloadShaders(container.Version) ? origin.PreloadShaders.ToArray() : Array.Empty<PPtr<Shader>>();
		}
	}
}
