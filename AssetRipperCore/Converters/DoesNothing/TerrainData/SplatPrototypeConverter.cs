﻿using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.TerrainData;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters.TerrainData
{
	public static class SplatPrototypeConverter
	{
		public static TerrainLayer GenerateTerrainLayer(IExportContainer container, ref SplatPrototype origin)
		{
			throw new System.NotImplementedException();
			/*TerrainLayer layer = new TerrainLayer();
			layer.DiffuseTexture = origin.Texture;
			layer.NormalMapTexture = origin.NormalMap;
			layer.TileSize = origin.TileSize;
			layer.TileOffset = origin.TileOffset;
			layer.Specular = new ColorRGBAf(origin.SpecularMetallic.X, origin.SpecularMetallic.Y, origin.SpecularMetallic.Z, 1.0f);
			layer.Metallic = origin.SpecularMetallic.W;
			layer.Smoothness = origin.Smoothness;
			return layer;*/
		}
	}
}
