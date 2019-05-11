using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Renderers;
using uTinyRipper.Classes.Terrains;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.AssetExporters.Classes;

namespace uTinyRipper.Classes
{
	public sealed class Terrain : Behaviour
	{
		public Terrain(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadShadowCastingMode(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadDrawHeightmap(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadDrawInstanced(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadReflectionProbeUsage(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.0.0f1 to 2019.2 exclusive
		/// </summary>
		public static bool IsReadMaterialType(Version version)
		{
			// unknown bottom version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final) && version.IsLess(2019, 2);
		}
		/// <summary>
		/// 5.0.0f1 to 5.0.1 exclusive
		/// </summary>
		public static bool IsReadDefaultSmoothness(Version version)
		{
			// unknown bottom version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final) && version.IsLess(5, 0, 1);
		}
		/// <summary>
		/// Release
		/// </summary>
		public static bool IsReadLightmapIndex(TransferInstructionFlags flags)
		{
			return flags.IsRelease();
		}
		/// <summary>
		/// 2017.2 and greater and Release
		/// </summary>
		public static bool IsReadExplicitProbeSetHash(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(2017, 2) && flags.IsRelease();
		}
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadBakeLightProbesForTrees(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadPreserveTreePrototypeLayers(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 2019.1 and greater and Not Release
		/// </summary>
		public static bool IsReadDeringLightProbesForTrees(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 5.0.0f1 and greater and Release
		/// </summary>
		public static bool IsReadDynamicUVST(Version version, TransferInstructionFlags flags)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final) && flags.IsRelease();
		}
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadScaleInLightmap(TransferInstructionFlags flags)
		{
			// unknown version
			return !flags.IsRelease();
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadGroupingID(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}

		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}

		private static int GetSerializedVersion(Version version)
		{
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
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Final))
			{
				return 3;
			}
			// unknown (beta) version
			// return 2;
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			TerrainData.Read(reader);
			TreeDistance = reader.ReadSingle();
			TreeBillboardDistance = reader.ReadSingle();
			TreeCrossFadeLength = reader.ReadSingle();
			TreeMaximumFullLODCount = reader.ReadInt32();
			DetailObjectDistance = reader.ReadSingle();
			DetailObjectDensity = reader.ReadSingle();
			HeightmapPixelError = reader.ReadSingle();
			SplatMapDistance = reader.ReadSingle();
			HeightmapMaximumLOD = reader.ReadInt32();
			if (IsReadShadowCastingMode(reader.Version))
			{
				ShadowCastingMode = (ShadowCastingMode)reader.ReadInt32();
			}
			else
			{
				bool CastShadows = reader.ReadBoolean();
				ShadowCastingMode = CastShadows ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off;
			}
			if (IsReadDrawHeightmap(reader.Version))
			{
				DrawHeightmap = reader.ReadBoolean();
			}
			if (IsReadDrawInstanced(reader.Version))
			{
				DrawInstanced = reader.ReadBoolean();
			}
			DrawTreesAndFoliage = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);

			if (IsReadReflectionProbeUsage(reader.Version))
			{
				ReflectionProbeUsage = (ReflectionProbeUsage)reader.ReadInt32();
			}
			if (IsReadMaterialType(reader.Version))
			{
				MaterialType = (MaterialType)reader.ReadInt32();
				LegacySpecular.Read(reader);
				LegacyShininess = reader.ReadSingle();
			}
			if (IsReadDefaultSmoothness(reader.Version))
			{
				UseDefaultSmoothness = reader.ReadBoolean();
				DefaultSmoothness = reader.ReadSingle();
			}
			MaterialTemplate.Read(reader);
			if (IsReadLightmapIndex(reader.Flags))
			{
				LightmapIndex = reader.ReadUInt16();
				LightmapIndexDynamic = reader.ReadUInt16();
				LightmapTilingOffset.Read(reader);
				LightmapTilingOffsetDynamic.Read(reader);
			}
			if (IsReadExplicitProbeSetHash(reader.Version, reader.Flags))
			{
				ExplicitProbeSetHash.Read(reader);
			}

			if (IsReadBakeLightProbesForTrees(reader.Version))
			{
				BakeLightProbesForTrees = reader.ReadBoolean();
			}
			if (IsReadPreserveTreePrototypeLayers(reader.Version))
			{
				PreserveTreePrototypeLayers = reader.ReadBoolean();
			}
			if (IsReadDeringLightProbesForTrees(reader.Version, reader.Flags))
			{
				DeringLightProbesForTrees = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
#if UNIVERSAL
			if (IsReadScaleInLightmap(reader.Flags))
			{
				ScaleInLightmap = reader.ReadSingle();
				LightmapParameters.Read(reader);
			}
#endif
			if (IsReadDynamicUVST(reader.Version, reader.Flags))
			{
				DynamicUVST.Read(reader);
				ChunkDynamicUVST.Read(reader);
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadGroupingID(reader.Version))
			{
				GroupingID = reader.ReadInt32();
				AllowAutoConnect = reader.ReadBoolean();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return TerrainData.FetchDependency(file, isLog, ToLogString, TerrainDataName);
			yield return MaterialTemplate.FetchDependency(file, isLog, ToLogString, MaterialTemplateName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(TerrainDataName, TerrainData.ExportYAML(container));
			node.Add(TreeDistanceName, TreeDistance);
			node.Add(TreeBillboardDistanceName, TreeBillboardDistance);
			node.Add(TreeCrossFadeLengthName, TreeCrossFadeLength);
			node.Add(TreeMaximumFullLODCountName, TreeMaximumFullLODCount);
			node.Add(DetailObjectDistanceName, DetailObjectDistance);
			node.Add(DetailObjectDensityName, DetailObjectDensity);
			node.Add(HeightmapPixelErrorName, HeightmapPixelError);
			node.Add(SplatMapDistanceName, SplatMapDistance);
			node.Add(HeightmapMaximumLODName, HeightmapMaximumLOD);
			if (IsReadShadowCastingMode(container.ExportVersion))
			{
				node.Add(ShadowCastingModeName, (int)ShadowCastingMode);
			}
			else
			{
				node.Add(CastShadowsName, CastShadows);
			}
			node.Add(DrawHeightmapName, DrawHeightmap);
			if (IsReadDrawInstanced(container.ExportVersion))
			{
				node.Add(DrawInstancedName, DrawInstanced);
			}
			node.Add(DrawTreesAndFoliageName, DrawTreesAndFoliage);
			node.Add(ReflectionProbeUsageName, (int)ReflectionProbeUsage);
			if (IsReadMaterialType(container.ExportVersion))
			{
				node.Add(MaterialTypeName, (int)GetMaterialType(container.Version));
				node.Add(LegacySpecularName, LegacySpecular.ExportYAML(container));
				node.Add(LegacyShininessName, LegacyShininess);
			}
			node.Add(MaterialTemplateName, ExportMaterialTemplate(container));
			node.Add(BakeLightProbesForTreesName, BakeLightProbesForTrees);
			if (IsReadPreserveTreePrototypeLayers(container.ExportVersion))
			{
				node.Add(PreserveTreePrototypeLayersName, PreserveTreePrototypeLayers);
			}
			if (IsReadDeringLightProbesForTrees(container.ExportVersion, container.ExportFlags))
			{
				node.Add(DeringLightProbesForTreesName, GetDeringLightProbesForTrees(container.Version, container.Flags));
			}
#warning TODO: get lightmap by index and fill those values
			node.Add(ScaleInLightmapName, GetScaleInLightmap(container.Flags));
			node.Add(LightmapParametersName, GetLightmapParameters(container.Flags).ExportYAML(container));
			if (IsReadGroupingID(container.ExportVersion))
			{
				node.Add(GroupingIDName, GroupingID);
				node.Add(AllowAutoConnectName, AllowAutoConnect);
			}
			return node;
		}

		private MaterialType GetMaterialType(Version version)
		{
			if (GetSerializedVersion(version) > 2)
			{
				return MaterialType;
			}
			return MaterialType == MaterialType.BuiltInStandard ? MaterialType.BuiltInLegacyDiffuse : MaterialType.Custom;
		}
		private YAMLNode ExportMaterialTemplate(IExportContainer container)
		{
			if (GetSerializedVersion(container.ExportVersion) < 5)
			{
				return MaterialTemplate.ExportYAML(container);
			}
			if (GetSerializedVersion(container.Version) >= 5)
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
		private bool GetDeringLightProbesForTrees(Version version, TransferInstructionFlags flags)
		{
			return IsReadDeringLightProbesForTrees(version, flags) ? DeringLightProbesForTrees : true;
		}
		private float GetScaleInLightmap(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadScaleInLightmap(flags))
			{
				return ScaleInLightmap;
			}
#endif
			return 0.0512f;
		}
		private PPtr<LightmapParameters> GetLightmapParameters(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadScaleInLightmap(flags))
			{
				return LightmapParameters;
			}
#endif
			return default;
		}

		public float TreeDistance { get; private set; }
		public float TreeBillboardDistance { get; private set; }
		public float TreeCrossFadeLength { get; private set; }
		public int TreeMaximumFullLODCount { get; private set; }
		public float DetailObjectDistance { get; private set; }
		public float DetailObjectDensity { get; private set; }
		public float HeightmapPixelError { get; private set; }
		public float SplatMapDistance { get; private set; }
		public int HeightmapMaximumLOD { get; private set; }
		public bool CastShadows => ShadowCastingMode != ShadowCastingMode.Off;
		public ShadowCastingMode ShadowCastingMode { get; private set; }
		public bool DrawHeightmap { get; private set; }
		public bool DrawInstanced { get; private set; }
		public bool DrawTreesAndFoliage { get; private set; }
		public ReflectionProbeUsage ReflectionProbeUsage { get; private set; }
		public MaterialType MaterialType { get; private set; }
		public float LegacyShininess { get; private set; }
		public bool UseDefaultSmoothness { get; private set; }
		public float DefaultSmoothness { get; private set; }
		public ushort LightmapIndex { get; private set; }
		public ushort LightmapIndexDynamic { get; private set; }
		public bool BakeLightProbesForTrees { get; private set; }
		public bool PreserveTreePrototypeLayers { get; private set; }
		public bool DeringLightProbesForTrees { get; private set; }
#if UNIVERSAL
		public float ScaleInLightmap { get; private set; }
#endif
		public int GroupingID { get; private set; }
		public bool AllowAutoConnect { get; private set; }

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
		public const string AllowAutoConnectName = "m_AllowAutoConnect";

		public PPtr<TerrainData> TerrainData;
		public ColorRGBA32 LegacySpecular;
		public PPtr<Material> MaterialTemplate;
		public Vector4f LightmapTilingOffset;
		public Vector4f LightmapTilingOffsetDynamic;
		public Hash128 ExplicitProbeSetHash;
#if UNIVERSAL
		public PPtr<LightmapParameters> LightmapParameters;
#endif
		public Vector4f DynamicUVST;
		public Vector4f ChunkDynamicUVST;
	}
}
