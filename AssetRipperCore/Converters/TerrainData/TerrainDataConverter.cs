using AssetRipper.Project;
using AssetRipper.Classes.Misc;
using System;
using System.Linq;

namespace AssetRipper.Converters.TerrainData
{
	public static class TerrainDataConverter
	{
		public static AssetRipper.Classes.TerrainData.TerrainData Convert(IExportContainer container, AssetRipper.Classes.TerrainData.TerrainData origin)
		{
			AssetRipper.Classes.TerrainData.TerrainData instance = new AssetRipper.Classes.TerrainData.TerrainData(origin.AssetInfo);
			NamedObjectConverter.Convert(container, origin, instance);
			instance.SplatDatabase = origin.SplatDatabase.Convert(container);
			instance.DetailDatabase = origin.DetailDatabase.Convert(container);
			instance.Heightmap = origin.Heightmap.Convert(container);
			if (AssetRipper.Classes.TerrainData.TerrainData.HasLightmap(container.ExportVersion))
			{
				instance.Lightmap = origin.Lightmap;
			}
			if (AssetRipper.Classes.TerrainData.TerrainData.HasPreloadShaders(container.ExportVersion))
			{
				instance.PreloadShaders = GetPreloadShaders(container, origin);
			}
			return instance;
		}

		private static PPtr<AssetRipper.Classes.Shader.Shader>[] GetPreloadShaders(IExportContainer container, AssetRipper.Classes.TerrainData.TerrainData origin)
		{
			return AssetRipper.Classes.TerrainData.TerrainData.HasPreloadShaders(container.Version) ? origin.PreloadShaders.ToArray() : Array.Empty<PPtr<AssetRipper.Classes.Shader.Shader>>();
		}
	}
}
