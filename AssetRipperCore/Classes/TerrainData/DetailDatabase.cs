using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.TerrainData
{
	public sealed class DetailDatabase : IAsset, IDependent
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			// Detail shaders has been added
			if (version.IsGreaterEqual(2019, 1, 0, UnityVersionType.Beta, 6))
			{
				return 3;
			}
			// unknown change
			if (version.IsGreaterEqual(5, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		public static bool HasAtlasTexture(UnityVersion version) => version.IsLess(2, 6);
		/// <summary>
		/// 2019.1.0b6 and greater
		/// </summary>
		public static bool HasDetailBillboardShader(UnityVersion version) => version.IsGreaterEqual(2019, 1, 0, UnityVersionType.Beta, 6);
		/// <summary>
		/// Less than 2020.2
		/// </summary>
		public static bool HasRandomRotations(UnityVersion version) => version.IsLess(2020, 2);

		public void Read(AssetReader reader)
		{
			Patches = reader.ReadAssetArray<DetailPatch>();
			DetailPrototypes = reader.ReadAssetArray<DetailPrototype>();
			PatchCount = reader.ReadInt32();
			PatchSamples = reader.ReadInt32();
			if (HasRandomRotations(reader.Version))
			{
				RandomRotations = reader.ReadAssetArray<Vector3f>();
			}

			if (HasAtlasTexture(reader.Version))
			{
				AtlasTexture.Read(reader);
			}

			WavingGrassTint.Read(reader);
			WavingGrassStrength = reader.ReadSingle();
			WavingGrassAmount = reader.ReadSingle();
			WavingGrassSpeed = reader.ReadSingle();
			if (HasDetailBillboardShader(reader.Version))
			{
				DetailBillboardShader.Read(reader);
				DetailMeshLitShader.Read(reader);
				DetailMeshGrassShader.Read(reader);
			}

			TreeDatabase.Read(reader);
			if (!HasAtlasTexture(reader.Version))
			{
				PreloadTextureAtlasData = reader.ReadAssetArray<PPtr<Texture2D.Texture2D>>();
			}
		}

		public void Write(AssetWriter writer)
		{
			Patches.Write(writer);
			DetailPrototypes.Write(writer);
			writer.Write(PatchCount);
			writer.Write(PatchSamples);
			if (HasRandomRotations(writer.Version))
			{
				RandomRotations.Write(writer);
			}

			if (HasAtlasTexture(writer.Version))
			{
				AtlasTexture.Write(writer);
			}

			WavingGrassTint.Write(writer);
			writer.Write(WavingGrassStrength);
			writer.Write(WavingGrassAmount);
			writer.Write(WavingGrassSpeed);
			if (HasDetailBillboardShader(writer.Version))
			{
				DetailBillboardShader.Write(writer);
				DetailMeshLitShader.Write(writer);
				DetailMeshGrassShader.Write(writer);
			}

			TreeDatabase.Write(writer);
			if (!HasAtlasTexture(writer.Version))
			{
				PreloadTextureAtlasData.Write(writer);
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(PatchesName, Patches.ExportYaml(container));
			node.Add(DetailPrototypesName, DetailPrototypes.ExportYaml(container));
			node.Add(PatchCountName, PatchCount);
			node.Add(PatchSamplesName, PatchSamples);
			if (HasRandomRotations(container.ExportVersion))
			{
				node.Add(RandomRotationsName, RandomRotations.ExportYaml(container));
			}

			if (HasAtlasTexture(container.ExportVersion))
			{
				node.Add(AtlasTextureName, AtlasTexture.ExportYaml(container));
			}

			node.Add(WavingGrassTintName, WavingGrassTint.ExportYaml(container));
			node.Add(WavingGrassStrengthName, WavingGrassStrength);
			node.Add(WavingGrassAmountName, WavingGrassAmount);
			node.Add(WavingGrassSpeedName, WavingGrassSpeed);
			if (HasDetailBillboardShader(container.ExportVersion))
			{
				node.Add(DetailBillboardShaderName, DetailBillboardShader.ExportYaml(container));
				node.Add(DetailMeshLitShaderName, DetailMeshLitShader.ExportYaml(container));
				node.Add(DetailMeshGrassShaderName, DetailMeshGrassShader.ExportYaml(container));
			}
			TreeDatabase.ExportYaml(container, node);
			if (!HasAtlasTexture(container.ExportVersion))
			{
				node.Add(PreloadTextureAtlasDataName, PreloadTextureAtlasData.ExportYaml(container));
			}
			return node;
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(DetailPrototypes, DetailPrototypesName))
			{
				yield return asset;
			}
			if (HasAtlasTexture(context.Version))
			{
				yield return context.FetchDependency(AtlasTexture, AtlasTextureName);
			}
			foreach (PPtr<IUnityObjectBase> asset in TreeDatabase.FetchDependencies(context))
			{
				yield return asset;
			}
			if (!HasAtlasTexture(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(PreloadTextureAtlasData, PreloadTextureAtlasDataName))
				{
					yield return asset;
				}
			}
		}

		/*private YamlNode ExportDetailBillboardShader(IExportContainer container)
		{
			if (HasDetailBillboardShader(container.Version))
			{
				return DetailBillboardShader.ExportYaml(container);
			}

			EngineBuiltInAsset buildInAsset = EngineBuiltInAssets.GetShader(EngineBuiltInAssets.TerrainBillboardWavingDoublePass, container.ExportVersion);
			return buildInAsset.ToExportPointer().ExportYaml(container);
		}
		private YamlNode ExportDetailMeshLitShader(IExportContainer container)
		{
			if (HasDetailBillboardShader(container.Version))
			{
				return DetailMeshLitShader.ExportYaml(container);
			}

			EngineBuiltInAsset buildInAsset = EngineBuiltInAssets.GetShader(EngineBuiltInAssets.TerrainVertexLit, container.ExportVersion);
			return buildInAsset.ToExportPointer().ExportYaml(container);
		}
		private YamlNode ExportDetailMeshGrassShader(IExportContainer container)
		{
			if (HasDetailBillboardShader(container.Version))
			{
				return DetailMeshGrassShader.ExportYaml(container);
			}

			EngineBuiltInAsset buildInAsset = EngineBuiltInAssets.GetShader(EngineBuiltInAssets.TerrainWavingDoublePass, container.ExportVersion);
			return buildInAsset.ToExportPointer().ExportYaml(container);
		}*/

		public DetailPatch[] Patches { get; set; }
		public DetailPrototype[] DetailPrototypes { get; set; }
		public int PatchCount { get; set; }
		public int PatchSamples { get; set; }
		public Vector3f[] RandomRotations { get; set; }
		public float WavingGrassStrength { get; set; }
		public float WavingGrassAmount { get; set; }
		public float WavingGrassSpeed { get; set; }
		public PPtr<Texture2D.Texture2D>[] PreloadTextureAtlasData { get; set; }

		public const string PatchesName = "m_Patches";
		public const string DetailPrototypesName = "m_DetailPrototypes";
		public const string PatchCountName = "m_PatchCount";
		public const string PatchSamplesName = "m_PatchSamples";
		public const string RandomRotationsName = "m_RandomRotations";
		public const string AtlasTextureName = "m_AtlasTexture";
		public const string WavingGrassTintName = "WavingGrassTint";
		public const string WavingGrassStrengthName = "m_WavingGrassStrength";
		public const string WavingGrassAmountName = "m_WavingGrassAmount";
		public const string WavingGrassSpeedName = "m_WavingGrassSpeed";
		public const string DetailBillboardShaderName = "m_DetailBillboardShader";
		public const string DetailMeshLitShaderName = "m_DetailMeshLitShader";
		public const string DetailMeshGrassShaderName = "m_DetailMeshGrassShader";
		public const string PreloadTextureAtlasDataName = "m_PreloadTextureAtlasData";

		public PPtr<Texture2D.Texture2D> AtlasTexture = new();
		public ColorRGBAf WavingGrassTint = new();
		public PPtr<Shader.Shader> DetailBillboardShader = new();
		public PPtr<Shader.Shader> DetailMeshLitShader = new();
		public PPtr<Shader.Shader> DetailMeshGrassShader = new();
		public TreeDatabase TreeDatabase = new();
	}
}
