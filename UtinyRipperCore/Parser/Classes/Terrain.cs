using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

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
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadDrawHeightmap(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
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
			DrawTreesAndFoliage = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);

			if (IsReadReflectionProbeUsage(reader.Version))
			{
				ReflectionProbeUsage = reader.ReadInt32();
				MaterialType = reader.ReadInt32();
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
			if(IsReadExplicitProbeSetHash(reader.Version))
			{
				ExplicitProbeSetHash.Read(reader);
			}

			if (IsReadBakeLightProbesForTrees(reader.Version))
			{
				BakeLightProbesForTrees = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			
				DynamicUVST.Read(reader);
				ChunkDynamicUVST.Read(reader);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			yield return TerrainData.FetchDependency(file, isLog, ToLogString, "m_TerrainData");
			yield return MaterialTemplate.FetchDependency(file, isLog, ToLogString, "m_MaterialTemplate");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_TerrainData", TerrainData.ExportYAML(container));
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
			node.Add("m_LegacySpecular", LegacySpecular.ExportYAML(container));
			node.Add("m_LegacyShininess", LegacyShininess);
			node.Add("m_MaterialTemplate", MaterialTemplate.ExportYAML(container));
			node.Add("m_BakeLightProbesForTrees", BakeLightProbesForTrees);
#warning TODO: get lightmap by index and fill those values
			node.Add("m_ScaleInLightmap", 0.0512f);
			node.Add("m_LightmapParameters", default(PPtr<Object>).ExportYAML(container));
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
