using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.TerrainData;
using AssetRipper.Core.Project;
using System;
using System.Linq;

namespace AssetRipper.Core.Converters.TerrainData
{
	public static class DetailDatabaseConverter
	{
		public static DetailDatabase Convert(IExportContainer container, DetailDatabase origin)
		{
			DetailDatabase instance = new DetailDatabase();
			instance.Patches = origin.Patches.ToArray();
			instance.DetailPrototypes = origin.DetailPrototypes.ToArray();
			instance.PatchCount = origin.PatchCount;
			instance.PatchSamples = origin.PatchSamples;
			instance.RandomRotations = origin.RandomRotations;
			instance.AtlasTexture = origin.AtlasTexture;
			instance.WavingGrassTint = origin.WavingGrassTint;
			instance.WavingGrassStrength = origin.WavingGrassStrength;
			instance.WavingGrassAmount = origin.WavingGrassAmount;
			instance.WavingGrassSpeed = origin.WavingGrassSpeed;
			if (DetailDatabase.HasDetailBillboardShader(container.ExportVersion))
			{
				instance.DetailBillboardShader = GetDetailBillboardShader(container, origin);
				instance.DetailMeshLitShader = GetDetailMeshLitShader(container, origin);
				instance.DetailMeshGrassShader = GetDetailMeshGrassShader(container, origin);
			}
			instance.TreeDatabase.TreeInstances = origin.TreeDatabase.TreeInstances.ToArray();
			instance.TreeDatabase.TreePrototypes = origin.TreeDatabase.TreePrototypes.ToArray();
			instance.PreloadTextureAtlasData = origin.PreloadTextureAtlasData.ToArray();
			return instance;
		}

		private static PPtr<AssetRipper.Core.Classes.Shader.Shader> GetDetailBillboardShader(IExportContainer container, DetailDatabase origin)
		{
			if (DetailDatabase.HasDetailBillboardShader(container.Version))
			{
				return origin.DetailBillboardShader;
			}
			else
			{
#warning TODO: add references to builtin shader
				throw new NotImplementedException();
			}
		}

		private static PPtr<AssetRipper.Core.Classes.Shader.Shader> GetDetailMeshLitShader(IExportContainer container, DetailDatabase origin)
		{
			if (DetailDatabase.HasDetailBillboardShader(container.Version))
			{
				return origin.DetailMeshLitShader;
			}
			else
			{
#warning TODO: add references to builtin shader
				throw new NotImplementedException();
			}
		}

		private static PPtr<AssetRipper.Core.Classes.Shader.Shader> GetDetailMeshGrassShader(IExportContainer container, DetailDatabase origin)
		{
			if (DetailDatabase.HasDetailBillboardShader(container.Version))
			{
				return origin.DetailMeshGrassShader;
			}
			else
			{
#warning TODO: add references to builtin shader
				throw new NotImplementedException();
			}
		}
	}
}
