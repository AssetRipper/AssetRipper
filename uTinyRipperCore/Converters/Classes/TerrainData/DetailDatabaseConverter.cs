using System;
using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.Classes.TerrainDatas;

namespace uTinyRipper.Converters.TerrainDatas
{
	public static class DetailDatabaseConverter
	{
		public static DetailDatabase Convert(IExportContainer container, ref DetailDatabase origin)
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
				instance.DetailBillboardShader = GetDetailBillboardShader(container, ref origin);
				instance.DetailMeshLitShader = GetDetailMeshLitShader(container, ref origin);
				instance.DetailMeshGrassShader = GetDetailMeshGrassShader(container, ref origin);
			}
			instance.TreeDatabase.TreeInstances = origin.TreeDatabase.TreeInstances.ToArray();
			instance.TreeDatabase.TreePrototypes = origin.TreeDatabase.TreePrototypes.ToArray();
			instance.PreloadTextureAtlasData = origin.PreloadTextureAtlasData.ToArray();
			return instance;
		}

		private static PPtr<Shader> GetDetailBillboardShader(IExportContainer container, ref DetailDatabase origin)
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

		private static PPtr<Shader> GetDetailMeshLitShader(IExportContainer container, ref DetailDatabase origin)
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

		private static PPtr<Shader> GetDetailMeshGrassShader(IExportContainer container, ref DetailDatabase origin)
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
