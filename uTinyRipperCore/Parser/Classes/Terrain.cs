using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Renderers;
using uTinyRipper.Classes.Terrains;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class Terrain : Behaviour
	{
		public Terrain(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0b1 to 5.0.1 exclusive
		/// </summary>
		public static bool IsReadDefaultSmoothness(Version version)
		{
#warning unknown bottom version
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1) && version.IsLess(5, 0, 1);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadDrawHeightmap(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadDrawInstanced(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadReflectionProbeUsage(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadExplicitProbeSetHash(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadBakeLightProbesForTrees(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadPreserveTreePrototypeLayers(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadDynamicUVST(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadGroupingID(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}

		private static int GetSerializedVersion(Version version)
		{
#warning unknown
			if (version.IsGreater(5, 0, 0, VersionType.Beta, 2))
			{
				return 3;
			}
#warning unknown
			if (version.IsGreater(5, 0, 0, VersionType.Beta, 1))
			{
				return 2;
			}
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
			CastShadows = reader.ReadBoolean();
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
			LightmapIndex = reader.ReadUInt16();
			LightmapIndexDynamic = reader.ReadUInt16();
			LightmapTilingOffset.Read(reader);
			LightmapTilingOffsetDynamic.Read(reader);
			if (IsReadExplicitProbeSetHash(reader.Version))
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
			if (IsReadDynamicUVST(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);

				DynamicUVST.Read(reader);
				ChunkDynamicUVST.Read(reader);
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
			node.Add(CastShadowsName, CastShadows);
			node.Add(DrawHeightmapName, DrawHeightmap);
			if (IsReadDrawInstanced(container.ExportVersion))
			{
				node.Add(DrawInstancedName, DrawInstanced);
			}
			node.Add(DrawTreesAndFoliageName, DrawTreesAndFoliage);
			node.Add(ReflectionProbeUsageName, (int)ReflectionProbeUsage);
			node.Add(MaterialTypeName, (int)MaterialType);
			node.Add(LegacySpecularName, LegacySpecular.ExportYAML(container));
			node.Add(LegacyShininessName, LegacyShininess);
			node.Add(MaterialTemplateName, MaterialTemplate.ExportYAML(container));
			node.Add(BakeLightProbesForTreesName, BakeLightProbesForTrees);
			if (IsReadPreserveTreePrototypeLayers(container.ExportVersion))
			{
				node.Add(PreserveTreePrototypeLayersName, PreserveTreePrototypeLayers);
			}
#warning TODO: get lightmap by index and fill those values
			node.Add(ScaleInLightmapName, 0.0512f);
			node.Add(LightmapParametersName, default(PPtr<Object>).ExportYAML(container));
			if (IsReadGroupingID(container.ExportVersion))
			{
				node.Add(GroupingIDName, GroupingID);
				node.Add(AllowAutoConnectName, AllowAutoConnect);
			}
			return node;

		}

		public byte Enabled { get; private set; }
		public float TreeDistance { get; private set; }
		public float TreeBillboardDistance { get; private set; }
		public float TreeCrossFadeLength { get; private set; }
		public int TreeMaximumFullLODCount { get; private set; }
		public float DetailObjectDistance { get; private set; }
		public float DetailObjectDensity { get; private set; }
		public float HeightmapPixelError { get; private set; }
		public float SplatMapDistance { get; private set; }
		public int HeightmapMaximumLOD { get; private set; }
		public bool CastShadows { get; private set; }
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
		public Vector4f DynamicUVST;
		public Vector4f ChunkDynamicUVST;
	}
}
