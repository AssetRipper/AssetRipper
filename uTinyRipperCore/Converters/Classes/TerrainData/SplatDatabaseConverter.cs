using System;
using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.Classes.TerrainDatas;

namespace uTinyRipper.Converters.TerrainDatas
{
	public static class SplatDatabaseConverter
	{
		public static SplatDatabase Convert(IExportContainer container, ref SplatDatabase origin)
		{
			SplatDatabase instance = new SplatDatabase();
			if (SplatDatabase.HasTerrainLayers(container.ExportVersion))
			{
				instance.TerrainLayers = GetTerrainLayers(container, ref origin);
			}
			else
			{
				instance.Splats = origin.Splats.ToArray();
			}
			instance.AlphaTextures = origin.AlphaTextures.ToArray();
			instance.AlphamapResolution = origin.AlphamapResolution;
			instance.BaseMapResolution = origin.BaseMapResolution;
			if (SplatDatabase.HasColorSpace(container.ExportVersion))
			{
				instance.ColorSpace = origin.ColorSpace;
				instance.MaterialRequiresMetallic = GetMaterialRequiresMetallic(container, ref origin);
				instance.MaterialRequiresSmoothness = GetMaterialRequiresSmoothness(container, ref origin);
			}
			return instance;
		}

		public static TerrainLayer[] GenerateTerrainLayers(IExportContainer container, ref SplatDatabase origin)
		{
			TerrainLayer[] layers = new TerrainLayer[origin.Splats.Length];
			for (int i = 0; i < layers.Length; i++)
			{
				layers[i] = origin.Splats[i].Convert(container);
			}
			return layers;
		}

		private static PPtr<TerrainLayer>[] GetTerrainLayers(IExportContainer container, ref SplatDatabase origin)
		{
			if (SplatDatabase.HasTerrainLayers(container.Version))
			{
				return origin.TerrainLayers.ToArray();
			}
			else
			{
#warning TODO: convert SplatPrototype to TerrainLayer and add new asset to new SerializedFile
				throw new NotImplementedException();
			}
		}

		private static bool GetMaterialRequiresMetallic(IExportContainer container, ref SplatDatabase origin)
		{
			return SplatDatabase.HasColorSpace(container.Version) ? origin.MaterialRequiresMetallic : true;
		}

		private static bool GetMaterialRequiresSmoothness(IExportContainer container, ref SplatDatabase origin)
		{
			return SplatDatabase.HasColorSpace(container.Version) ? origin.MaterialRequiresSmoothness : true;
		}
	}
}
