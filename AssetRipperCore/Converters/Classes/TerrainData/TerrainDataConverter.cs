using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Misc;
using System;
using System.Linq;

namespace AssetRipper.Converters.Classes.TerrainData
{
	public static class TerrainDataConverter
	{
		public static Parser.Classes.TerrainData.TerrainData Convert(IExportContainer container, Parser.Classes.TerrainData.TerrainData origin)
		{
			Parser.Classes.TerrainData.TerrainData instance = new Parser.Classes.TerrainData.TerrainData(origin.AssetInfo);
			NamedObjectConverter.Convert(container, origin, instance);
			instance.SplatDatabase = origin.SplatDatabase.Convert(container);
			instance.DetailDatabase = origin.DetailDatabase.Convert(container);
			instance.Heightmap = origin.Heightmap.Convert(container);
			if (Parser.Classes.TerrainData.TerrainData.HasLightmap(container.ExportVersion))
			{
				instance.Lightmap = origin.Lightmap;
			}
			if (Parser.Classes.TerrainData.TerrainData.HasPreloadShaders(container.ExportVersion))
			{
				instance.PreloadShaders = GetPreloadShaders(container, origin);
			}
			return instance;
		}

		private static PPtr<Parser.Classes.Shader.Shader>[] GetPreloadShaders(IExportContainer container, Parser.Classes.TerrainData.TerrainData origin)
		{
			return Parser.Classes.TerrainData.TerrainData.HasPreloadShaders(container.Version) ? origin.PreloadShaders.ToArray() : Array.Empty<PPtr<Parser.Classes.Shader.Shader>>();
		}
	}
}
