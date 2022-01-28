using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Renderer;
using AssetRipper.Core.Classes.TerrainData;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Exporters.Engine;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Terrain
{
	public sealed class Terrain : Behaviour, ITerrain
	{
		public Terrain(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// RenderingLayerMask value has been changed from 1 to custom
			if (version.IsGreaterEqual(2019, 3))
			{
				return 6;
			}
			// MaterialType has been replaced by actual shaders
			if (version.IsGreaterEqual(2019, 2))
			{
				return 5;
			}
			// CastShadows has been converted to ShadowCastingMode
			if (version.IsGreaterEqual(2019))
			{
				return 4;
			}
			// unknown version
			if (version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final))
			{
				return 3;
			}
			// unknown (beta) version
			// return 2;
			return 1;
		}

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasShadowCastingMode(UnityVersion version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool HasDrawHeightmap(UnityVersion version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasDrawInstanced(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool HasReflectionProbeUsage(UnityVersion version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		}
		/// <summary>
		/// 5.0.0f1 to 2019.2 exclusive
		/// </summary>
		public static bool HasMaterialType(UnityVersion version)
		{
			// unknown bottom version
			return version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final) && version.IsLess(2019, 2);
		}
		/// <summary>
		/// 5.0.0f1 to 5.0.1 exclusive
		/// </summary>
		public static bool HasDefaultSmoothness(UnityVersion version) => version.IsEqual(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// Release
		/// </summary>
		public static bool HasLightmapIndex(TransferInstructionFlags flags)
		{
			return flags.IsRelease();
		}
		/// <summary>
		/// 2017.2 and greater and Release
		/// </summary>
		public static bool HasExplicitProbeSetHash(UnityVersion version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(2017, 2) && flags.IsRelease();
		}
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasBakeLightProbesForTrees(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasPreserveTreePrototypeLayers(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2019.1 and greater and Not Release
		/// </summary>
		public static bool HasDeringLightProbesForTrees(UnityVersion version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 5.0.0f1 and greater and Release (NOTE: unknown version)
		/// </summary>
		public static bool HasDynamicUVST(UnityVersion version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final) && flags.IsRelease();
		}
		/// <summary>
		/// Not Release (NOTE: unknown version)
		/// </summary>
		public static bool HasScaleInLightmap(TransferInstructionFlags flags) => !flags.IsRelease();
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasGroupingID(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasRenderingLayerMask(UnityVersion version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// 2021 and greater
		/// </summary>
		public static bool HasStaticShadowCaster(UnityVersion version) => version.IsGreaterEqual(2021);

		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_TerrainData.Read(reader);
			TreeDistance = reader.ReadSingle();
			TreeBillboardDistance = reader.ReadSingle();
			TreeCrossFadeLength = reader.ReadSingle();
			TreeMaximumFullLODCount = reader.ReadInt32();
			DetailObjectDistance = reader.ReadSingle();
			DetailObjectDensity = reader.ReadSingle();
			HeightmapPixelError = reader.ReadSingle();
			SplatMapDistance = reader.ReadSingle();
			HeightmapMaximumLOD = reader.ReadInt32();
			if (HasShadowCastingMode(reader.Version))
			{
				ShadowCastingMode = (ShadowCastingMode)reader.ReadInt32();
			}
			else
			{
				bool CastShadows = reader.ReadBoolean();
				ShadowCastingMode = CastShadows ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off;
			}
			if (HasDrawHeightmap(reader.Version))
			{
				DrawHeightmap = reader.ReadBoolean();
			}
			if (HasDrawInstanced(reader.Version))
			{
				DrawInstanced = reader.ReadBoolean();
			}
			DrawTreesAndFoliage = reader.ReadBoolean();

			if (HasStaticShadowCaster(reader.Version))
			{
				StaticShadowCaster = reader.ReadBoolean();
			}

			reader.AlignStream();

			if (HasReflectionProbeUsage(reader.Version))
			{
				ReflectionProbeUsage = (ReflectionProbeUsage)reader.ReadInt32();
			}
			if (HasMaterialType(reader.Version))
			{
				MaterialType = (MaterialType)reader.ReadInt32();
				LegacySpecular.Read(reader);
				LegacyShininess = reader.ReadSingle();
			}
			if (HasDefaultSmoothness(reader.Version))
			{
				UseDefaultSmoothness = reader.ReadBoolean();
				DefaultSmoothness = reader.ReadSingle();
			}
			MaterialTemplate.Read(reader);
			if (HasLightmapIndex(reader.Flags))
			{
				LightmapIndex = reader.ReadUInt16();
				LightmapIndexDynamic = reader.ReadUInt16();
				LightmapTilingOffset.Read(reader);
				LightmapTilingOffsetDynamic.Read(reader);
			}
			if (HasExplicitProbeSetHash(reader.Version, reader.Flags))
			{
				ExplicitProbeSetHash.Read(reader);
			}

			if (HasBakeLightProbesForTrees(reader.Version))
			{
				BakeLightProbesForTrees = reader.ReadBoolean();
			}
			if (HasPreserveTreePrototypeLayers(reader.Version))
			{
				PreserveTreePrototypeLayers = reader.ReadBoolean();
			}
			if (HasDeringLightProbesForTrees(reader.Version, reader.Flags))
			{
				DeringLightProbesForTrees = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}
			if (HasDynamicUVST(reader.Version, reader.Flags))
			{
				DynamicUVST.Read(reader);
				ChunkDynamicUVST.Read(reader);
				reader.AlignStream();
			}
			if (HasGroupingID(reader.Version))
			{
				GroupingID = reader.ReadInt32();
			}
			if (HasRenderingLayerMask(reader.Version))
			{
				RenderingLayerMask = reader.ReadUInt32();
			}
			if (HasGroupingID(reader.Version))
			{
				AllowAutoConnect = reader.ReadBoolean();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(m_TerrainData, TerrainDataName);
			yield return context.FetchDependency(MaterialTemplate, MaterialTemplateName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TerrainDataName, m_TerrainData.ExportYAML(container));
			node.Add(TreeDistanceName, TreeDistance);
			node.Add(TreeBillboardDistanceName, TreeBillboardDistance);
			node.Add(TreeCrossFadeLengthName, TreeCrossFadeLength);
			node.Add(TreeMaximumFullLODCountName, TreeMaximumFullLODCount);
			node.Add(DetailObjectDistanceName, DetailObjectDistance);
			node.Add(DetailObjectDensityName, DetailObjectDensity);
			node.Add(HeightmapPixelErrorName, HeightmapPixelError);
			node.Add(SplatMapDistanceName, SplatMapDistance);
			node.Add(HeightmapMaximumLODName, HeightmapMaximumLOD);
			if (HasShadowCastingMode(container.ExportVersion))
			{
				node.Add(ShadowCastingModeName, (int)ShadowCastingMode);
			}
			else
			{
				node.Add(CastShadowsName, CastShadows);
			}
			node.Add(DrawHeightmapName, DrawHeightmap);
			if (HasDrawInstanced(container.ExportVersion))
			{
				node.Add(DrawInstancedName, DrawInstanced);
			}
			node.Add(DrawTreesAndFoliageName, DrawTreesAndFoliage);
			if (HasStaticShadowCaster(container.ExportVersion))
			{
				node.Add(StaticShadowCasterName, StaticShadowCaster);
			}
			node.Add(ReflectionProbeUsageName, (int)ReflectionProbeUsage);
			if (HasMaterialType(container.ExportVersion))
			{
				node.Add(MaterialTypeName, (int)GetMaterialType(container.Version));
				node.Add(LegacySpecularName, LegacySpecular.ExportYAML(container));
				node.Add(LegacyShininessName, LegacyShininess);
			}
			node.Add(MaterialTemplateName, ExportMaterialTemplate(container));
			node.Add(BakeLightProbesForTreesName, BakeLightProbesForTrees);
			if (HasPreserveTreePrototypeLayers(container.ExportVersion))
			{
				node.Add(PreserveTreePrototypeLayersName, PreserveTreePrototypeLayers);
			}
			if (HasDeringLightProbesForTrees(container.ExportVersion, container.ExportFlags))
			{
				node.Add(DeringLightProbesForTreesName, GetDeringLightProbesForTrees(container.Version, container.Flags));
			}
#warning TODO: get lightmap by index and fill those values
			node.Add(ScaleInLightmapName, GetScaleInLightmap(container.Flags));
			node.Add(LightmapParametersName, GetLightmapParameters(container.Flags).ExportYAML(container));
			if (HasGroupingID(container.ExportVersion))
			{
				node.Add(GroupingIDName, GroupingID);
			}
			if (HasRenderingLayerMask(container.ExportVersion))
			{
				node.Add(RenderingLayerMaskName, GetRenderingLayerMask(container.Version));
			}
			if (HasGroupingID(container.ExportVersion))
			{
				node.Add(AllowAutoConnectName, AllowAutoConnect);
			}
			return node;
		}

		private MaterialType GetMaterialType(UnityVersion version)
		{
			if (ToSerializedVersion(version) > 2)
			{
				return MaterialType;
			}
			return MaterialType == MaterialType.BuiltInStandard ? MaterialType.BuiltInLegacyDiffuse : MaterialType.Custom;
		}
		private YAMLNode ExportMaterialTemplate(IExportContainer container)
		{
			if (ToSerializedVersion(container.ExportVersion) < 5)
			{
				return MaterialTemplate.ExportYAML(container);
			}
			if (ToSerializedVersion(container.Version) >= 5)
			{
				return MaterialTemplate.ExportYAML(container);
			}

			MaterialType type = GetMaterialType(container.Version);
			if (type == MaterialType.BuiltInStandard)
			{
				EngineBuiltInAsset asset = EngineBuiltInAssets.GetMaterial(EngineBuiltInAssets.DefaultTerrainStandardName, container.ExportVersion);
				return asset.ToExportPointer().ExportYAML(container);
			}
			if (type == MaterialType.BuiltInLegacyDiffuse)
			{
				EngineBuiltInAsset asset = EngineBuiltInAssets.GetMaterial(EngineBuiltInAssets.DefaultTerrainDiffuseName, container.ExportVersion);
				return asset.ToExportPointer().ExportYAML(container);
			}
			if (type == MaterialType.BuiltInLegacySpecular)
			{
				EngineBuiltInAsset asset = EngineBuiltInAssets.GetMaterial(EngineBuiltInAssets.DefaultTerrainSpecularName, container.ExportVersion);
				return asset.ToExportPointer().ExportYAML(container);
			}
			return MaterialTemplate.ExportYAML(container);
		}
		private bool GetDeringLightProbesForTrees(UnityVersion version, TransferInstructionFlags flags)
		{
			return HasDeringLightProbesForTrees(version, flags) ? DeringLightProbesForTrees : true;
		}
		private float GetScaleInLightmap(TransferInstructionFlags flags)
		{
			return 0.0512f;
		}
		private PPtr<LightmapParameters> GetLightmapParameters(TransferInstructionFlags flags)
		{
			return new();
		}
		private uint GetRenderingLayerMask(UnityVersion version)
		{
			return HasRenderingLayerMask(version) ? RenderingLayerMask : 1;
		}

		public float TreeDistance { get; set; }
		public float TreeBillboardDistance { get; set; }
		public float TreeCrossFadeLength { get; set; }
		public int TreeMaximumFullLODCount { get; set; }
		public float DetailObjectDistance { get; set; }
		public float DetailObjectDensity { get; set; }
		public float HeightmapPixelError { get; set; }
		public float SplatMapDistance { get; set; }
		public int HeightmapMaximumLOD { get; set; }
		public bool CastShadows => ShadowCastingMode != ShadowCastingMode.Off;
		public ShadowCastingMode ShadowCastingMode { get; set; }
		public bool DrawHeightmap { get; set; }
		public bool DrawInstanced { get; set; }
		public bool DrawTreesAndFoliage { get; set; }
		public bool StaticShadowCaster { get; set; }
		public ReflectionProbeUsage ReflectionProbeUsage { get; set; }
		public MaterialType MaterialType { get; set; }
		public float LegacyShininess { get; set; }
		public bool UseDefaultSmoothness { get; set; }
		public float DefaultSmoothness { get; set; }
		public ushort LightmapIndex { get; set; }
		public ushort LightmapIndexDynamic { get; set; }
		public bool BakeLightProbesForTrees { get; set; }
		public bool PreserveTreePrototypeLayers { get; set; }
		public bool DeringLightProbesForTrees { get; set; }
		public int GroupingID { get; set; }
		public uint RenderingLayerMask { get; set; }
		public bool AllowAutoConnect { get; set; }

		public PPtr<ITerrainData> TerrainData => m_TerrainData.CastTo<ITerrainData>();

		public const string TerrainDataName = "m_TerrainData";
		public const string TreeDistanceName = "m_TreeDistance";
		public const string TreeBillboardDistanceName = "m_TreeBillboardDistance";
		public const string TreeCrossFadeLengthName = "m_TreeCrossFadeLength";
		public const string TreeMaximumFullLODCountName = "m_TreeMaximumFullLODCount";
		public const string DetailObjectDistanceName = "m_DetailObjectDistance";
		public const string DetailObjectDensityName = "m_DetailObjectDensity";
		public const string HeightmapPixelErrorName = "m_HeightmapPixelError";
		public const string SplatMapDistanceName = "m_SplatMapDistance";
		public const string HeightmapMaximumLODName = "m_HeightmapMaximumLOD";
		public const string CastShadowsName = "m_CastShadows";
		public const string ShadowCastingModeName = "m_ShadowCastingMode";
		public const string DrawHeightmapName = "m_DrawHeightmap";
		public const string DrawInstancedName = "m_DrawInstanced";
		public const string DrawTreesAndFoliageName = "m_DrawTreesAndFoliage";
		public const string StaticShadowCasterName = "m_StaticShadowCaster";
		public const string ReflectionProbeUsageName = "m_ReflectionProbeUsage";
		public const string MaterialTypeName = "m_MaterialType";
		public const string LegacySpecularName = "m_LegacySpecular";
		public const string LegacyShininessName = "m_LegacyShininess";
		public const string MaterialTemplateName = "m_MaterialTemplate";
		public const string BakeLightProbesForTreesName = "m_BakeLightProbesForTrees";
		public const string PreserveTreePrototypeLayersName = "m_PreserveTreePrototypeLayers";
		public const string DeringLightProbesForTreesName = "m_DeringLightProbesForTrees";
		public const string ScaleInLightmapName = "m_ScaleInLightmap";
		public const string LightmapParametersName = "m_LightmapParameters";
		public const string GroupingIDName = "m_GroupingID";
		public const string RenderingLayerMaskName = "m_RenderingLayerMask";
		public const string AllowAutoConnectName = "m_AllowAutoConnect";

		public PPtr<TerrainData.TerrainData> m_TerrainData = new();
		public ColorRGBA32 LegacySpecular = new();
		public PPtr<Material.Material> MaterialTemplate = new();
		public Vector4f LightmapTilingOffset = new();
		public Vector4f LightmapTilingOffsetDynamic = new();
		public Hash128 ExplicitProbeSetHash = new();
		public Vector4f DynamicUVST = new();
		public Vector4f ChunkDynamicUVST = new();
	}
}
