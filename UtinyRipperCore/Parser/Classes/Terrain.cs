using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
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
		public static bool IsReadDrawHeightmap(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		public static bool IsReadReflectionProbeUsage(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		public static bool IsReadBakeLightProbesForTrees(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 3;
			}

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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			TerrainData.Read(stream);
			TreeDistance = stream.ReadSingle();
			TreeBillboardDistance = stream.ReadSingle();
			TreeCrossFadeLength = stream.ReadSingle();
			TreeMaximumFullLODCount = stream.ReadInt32();
			DetailObjectDistance = stream.ReadSingle();
			DetailObjectDensity = stream.ReadSingle();
			HeightmapPixelError = stream.ReadSingle();
			SplatMapDistance = stream.ReadSingle();
			HeightmapMaximumLOD = stream.ReadInt32();
			CastShadows = stream.ReadBoolean();
			if (IsReadDrawHeightmap(stream.Version))
			{
				DrawHeightmap = stream.ReadBoolean();
			}
			DrawTreesAndFoliage = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);

			if (IsReadReflectionProbeUsage(stream.Version))
			{
				ReflectionProbeUsage = stream.ReadInt32();
				MaterialType = stream.ReadInt32();
				LegacySpecular.Read(stream);
				LegacyShininess = stream.ReadSingle();
			}
			if (IsReadDefaultSmoothness(stream.Version))
			{
				UseDefaultSmoothness = stream.ReadBoolean();
				DefaultSmoothness = stream.ReadSingle();
			}
			MaterialTemplate.Read(stream);
			LightmapIndex = stream.ReadUInt16();
			LightmapIndexDynamic = stream.ReadUInt16();
			LightmapTilingOffset.Read(stream);
			LightmapTilingOffsetDynamic.Read(stream);
			ExplicitProbeSetHash.Read(stream);

			if (IsReadBakeLightProbesForTrees(stream.Version))
			{
				BakeLightProbesForTrees = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			
				DynamicUVST.Read(stream);
				ChunkDynamicUVST.Read(stream);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_TerrainData", TerrainData.ExportYAML(exporter));
			node.Add("m_TreeDistance", TreeDistance);
			node.Add("m_TreeBillboardDistance", TreeBillboardDistance);
			node.Add("m_TreeCrossFadeLength", TreeCrossFadeLength);
			node.Add("m_TreeMaximumFullLODCount", TreeMaximumFullLODCount);
			node.Add("m_DetailObjectDistance", DetailObjectDistance);
			node.Add("m_DetailObjectDensity", DetailObjectDensity);
			node.Add("m_HeightmapPixelError", HeightmapPixelError);
			node.Add("m_SplatMapDistance", SplatMapDistance);
			node.Add("m_HeightmapMaximumLOD", HeightmapMaximumLOD);
			node.Add("m_CastShadows", CastShadows);
			node.Add("m_DrawHeightmap", DrawHeightmap);
			node.Add("m_DrawTreesAndFoliage", DrawTreesAndFoliage);
			node.Add("m_ReflectionProbeUsage", ReflectionProbeUsage);
			node.Add("m_MaterialType", MaterialType);
			node.Add("m_LegacySpecular", LegacySpecular.ExportYAML(exporter));
			node.Add("m_LegacyShininess", LegacyShininess);
			node.Add("m_MaterialTemplate", MaterialTemplate.ExportYAML(exporter));
			node.Add("m_BakeLightProbesForTrees", BakeLightProbesForTrees);
#warning TODO: get lightmap by index and fill those values
			node.Add("m_ScaleInLightmap", 0.0512f);
			node.Add("m_LightmapParameters", default(PPtr<Object>).ExportYAML(exporter));
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
		public bool DrawTreesAndFoliage { get; private set; }
		public int ReflectionProbeUsage { get; private set; }
		public int MaterialType { get; private set; }
		public float LegacyShininess { get; private set; }
		public bool UseDefaultSmoothness { get; private set; }
		public float DefaultSmoothness { get; private set; }
		public ushort LightmapIndex { get; private set; }
		public ushort LightmapIndexDynamic { get; private set; }
		public bool BakeLightProbesForTrees { get; private set; }

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
