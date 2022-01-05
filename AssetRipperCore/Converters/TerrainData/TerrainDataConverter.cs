using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Project;
using System;
using System.Linq;

namespace AssetRipper.Core.Converters.TerrainData
{
	public static class TerrainDataConverter
	{
		public static AssetRipper.Core.Classes.TerrainData.TerrainData Convert(IExportContainer container, AssetRipper.Core.Classes.TerrainData.TerrainData origin)
		{
			AssetRipper.Core.Classes.TerrainData.TerrainData instance = new AssetRipper.Core.Classes.TerrainData.TerrainData(origin.AssetInfo);
			NamedObjectConverter.Convert(container, origin, instance);
			instance.SplatDatabase = origin.SplatDatabase.Convert(container);
			instance.DetailDatabase = origin.DetailDatabase.Convert(container);
			instance.m_Heightmap = origin.m_Heightmap.Convert(container);
			if (AssetRipper.Core.Classes.TerrainData.TerrainData.HasLightmap(container.ExportVersion))
			{
				instance.Lightmap = origin.Lightmap;
			}
			if (AssetRipper.Core.Classes.TerrainData.TerrainData.HasPreloadShaders(container.ExportVersion))
			{
				instance.PreloadShaders = GetPreloadShaders(container, origin);
			}
			return instance;
		}

		private static PPtr<AssetRipper.Core.Classes.Shader.Shader>[] GetPreloadShaders(IExportContainer container, AssetRipper.Core.Classes.TerrainData.TerrainData origin)
		{
			return AssetRipper.Core.Classes.TerrainData.TerrainData.HasPreloadShaders(container.Version) ? origin.PreloadShaders.ToArray() : Array.Empty<PPtr<AssetRipper.Core.Classes.Shader.Shader>>();
		}
	}
}
