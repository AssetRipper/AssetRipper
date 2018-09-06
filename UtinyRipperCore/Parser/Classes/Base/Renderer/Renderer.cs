using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.MeshRenderers;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public abstract class Renderer : Component
	{
		protected Renderer(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadDynamicOccludee(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadMotionVector(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadRenderingLayerMask(Version version)
		{
			return version.IsGreaterEqual(2018);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool IsReadLightmapIndex(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLightDynamic(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool IsReadTileOffset(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadTileDynamic(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 3.0.0 to 5.5.0
		/// </summary>
		public static bool IsReadSubsetIndices(Version version)
		{
			return version.IsGreaterEqual(3) && version.IsLess(5, 5);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadStaticBatchInfo(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 3.0.0
		/// </summary>
		public static bool IsReadStaticBatchRoot(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 3.5.0 to 5.4.0
		/// </summary>
		public static bool IsReadUseLight(Version version)
		{
			return version.IsGreaterEqual(3, 5) && version.IsLess(5, 4);
		}
		/// <summary>
		/// 5.0.0 to 5.3.x
		/// </summary>
		public static bool IsReadReflectUsage(Version version)
		{
			return version.IsGreaterEqual(5, 0) && version.IsLess(5, 4);
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadProbeAnchor(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadLightOverride(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool IsReadSortingLayerID(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 4.3.x or 5.6.0 and greater
		/// </summary>
		public static bool IsReadSortingLayer(Version version)
		{
			return version.IsEqual(4, 3) || version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadSortingOrder(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		
		/// <summary>
		/// 5.0.0 to 5.3.x
		/// </summary>
		private static bool IsAlignEnabled(Version version)
		{
			return version.IsGreaterEqual(5) && version.IsLess(5, 4);
		}
		/// <summary>
		/// Less 5.0.0
		/// </summary>
		private static bool IsByteLightIndex(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		private static bool IsReadMaterialFirst(Version version)
		{
			return version.IsLess(3);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsAlignLightProbe(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		private static bool IsAlignSortingOrder(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Enabled = reader.ReadBoolean();
			if(IsAlignEnabled(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			CastShadows = reader.ReadByte();
			ReceiveShadows = reader.ReadByte();
			if (IsAlignEnabled(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadMotionVector(reader.Version))
			{
				MotionVectors = reader.ReadByte();
				LightProbeUsage = reader.ReadByte();
				ReflectionProbeUsage = reader.ReadByte();
				reader.AlignStream(AlignType.Align4);
			}

			if(IsReadRenderingLayerMask(reader.Version))
			{
				RenderingLayerMask = reader.ReadUInt32();
			}
			if (IsReadLightmapIndex(reader.Version))
			{
				LightmapIndex = IsByteLightIndex(reader.Version) ? reader.ReadByte() : reader.ReadUInt16();
			}

			if(IsReadLightDynamic(reader.Version))
			{
				LightmapIndexDynamic = reader.ReadUInt16();
			}

			if (IsReadMaterialFirst(reader.Version))
			{
				m_materials = reader.ReadArray<PPtr<Material>>();
			}

			if (IsReadTileOffset(reader.Version))
			{
				LightmapTilingOffset.Read(reader);
			}
			if (IsReadTileDynamic(reader.Version))
			{
				LightmapTilingOffsetDynamic.Read(reader);
			}
			
			if (!IsReadMaterialFirst(reader.Version))
			{
				m_materials = reader.ReadArray<PPtr<Material>>();
			}

			if (IsReadSubsetIndices(reader.Version))
			{
				m_subsetIndices = reader.ReadUInt32Array();
			}
			if(IsReadStaticBatchInfo(reader.Version))
			{
				StaticBatchInfo.Read(reader);
			}

			if (IsReadStaticBatchRoot(reader.Version))
			{
				StaticBatchRoot.Read(reader);
			}

			if (IsReadUseLight(reader.Version))
			{
				UseLightProbes = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadReflectUsage(reader.Version))
			{
				ReflectionProbeUsage = reader.ReadInt32();
			}

			if (IsReadProbeAnchor(reader.Version))
			{
				ProbeAnchor.Read(reader);
			}
			if (IsReadLightOverride(reader.Version))
			{
				LightProbeVolumeOverride.Read(reader);
			}
			if (IsAlignLightProbe(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadSortingLayerID(reader.Version))
			{
				SortingLayerID = reader.ReadInt32();
			}
			if (IsReadSortingLayer(reader.Version))
			{
				SortingLayer = reader.ReadInt16();
			}
			if (IsReadSortingOrder(reader.Version))
			{
				SortingOrder = reader.ReadInt16();
			}
			if (IsAlignSortingOrder(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			foreach (PPtr<Material> material in Materials)
			{
				yield return material.FetchDependency(file, isLog, ToLogString, "m_Materials");
			}
			if (!StaticBatchRoot.IsNull)
			{
				yield return StaticBatchRoot.GetAsset(file);
			}
			if(!ProbeAnchor.IsNull)
			{
				yield return ProbeAnchor.GetAsset(file);
			}
			if(!LightProbeVolumeOverride.IsNull)
			{
				yield return LightProbeVolumeOverride.GetAsset(file);
			}
		}

		protected void ReadBase(AssetReader reader)
		{
			base.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: check undefined vars
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Enabled", Enabled);
			node.Add("m_CastShadows", CastShadows);
			node.Add("m_ReceiveShadows", ReceiveShadows);
			if(IsReadDynamicOccludee(container.Version))
			{
#warning TODO:
				node.Add("m_DynamicOccludee", 1);
			}
			else
			{
				node.Add("m_DynamicOccludee", 1);
			}
			node.Add("m_MotionVectors", MotionVectors);
			node.Add("m_LightProbeUsage", LightProbeUsage);
			node.Add("m_ReflectionProbeUsage", ReflectionProbeUsage);
			node.Add("m_Materials", Materials.ExportYAML(container));
			if (IsReadSubsetIndices(container.Version))
			{
				StaticBatchInfo staticBatchInfo = new StaticBatchInfo(SubsetIndices);
				node.Add("m_StaticBatchInfo", staticBatchInfo.ExportYAML(container));
			}
			else
			{
				node.Add("m_StaticBatchInfo", StaticBatchInfo.ExportYAML(container));
			}
			node.Add("m_StaticBatchRoot", StaticBatchRoot.ExportYAML(container));
			node.Add("m_ProbeAnchor", ProbeAnchor.ExportYAML(container));
			node.Add("m_LightProbeVolumeOverride", LightProbeVolumeOverride.ExportYAML(container));
#warning what are those vars?
			node.Add("m_ScaleInLightmap", 1);
			node.Add("m_PreserveUVs", 0);
			node.Add("m_IgnoreNormalsForChartDetection", 0);
			node.Add("m_ImportantGI", 0);
#warning TODO? Should I read this parameter or just write default value?
			node.Add("m_StitchLightmapSeams", 0);
			node.Add("m_SelectedEditorRenderState", 3);
			node.Add("m_MinimumChartSize", 4);
			node.Add("m_AutoUVMaxDistance", 0.5f);
			node.Add("m_AutoUVMaxAngle", 89);
#warning TODO?
			node.Add("m_LightmapParameters", default(PPtr<Object>).ExportYAML(container));
			node.Add("m_SortingLayerID", SortingLayerID);
			node.Add("m_SortingLayer", SortingLayer);
			node.Add("m_SortingOrder", SortingOrder);
			return node;
		}

		public bool Enabled { get; private set; }
		public byte CastShadows { get; private set; }
		public byte ReceiveShadows { get; private set; }
		public byte MotionVectors { get; private set; }
		public byte LightProbeUsage { get; private set; }
		public int ReflectionProbeUsage { get; private set; }
		public uint RenderingLayerMask { get; private set; }
		public ushort LightmapIndex { get; private set; }
		public ushort LightmapIndexDynamic { get; private set; }
		public IReadOnlyList<PPtr<Material>> Materials => m_materials;
		public IReadOnlyList<uint> SubsetIndices => m_subsetIndices;
		public bool UseLightProbes { get; private set; }
		public int SortingLayerID { get; private set; }
		public short SortingLayer { get; private set; }
		public short SortingOrder { get; private set; }

		public Vector4f LightmapTilingOffset;
		public Vector4f LightmapTilingOffsetDynamic;
		public StaticBatchInfo StaticBatchInfo;
		public PPtr<Transform> StaticBatchRoot;
		/// <summary>
		/// LightProbeAnchor previously
		/// </summary>
		public PPtr<Transform> ProbeAnchor;
		public PPtr<GameObject> LightProbeVolumeOverride;
		
		private PPtr<Material>[] m_materials;
		private uint[] m_subsetIndices;
	}
}
