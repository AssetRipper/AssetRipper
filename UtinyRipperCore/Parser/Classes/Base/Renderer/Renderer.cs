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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Enabled = stream.ReadBoolean();
			if(IsAlignEnabled(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			CastShadows = stream.ReadByte();
			ReceiveShadows = stream.ReadByte();
			if (IsAlignEnabled(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if (IsReadMotionVector(stream.Version))
			{
				MotionVectors = stream.ReadByte();
				LightProbeUsage = stream.ReadByte();
				ReflectionProbeUsage = stream.ReadByte();
				stream.AlignStream(AlignType.Align4);
			}

			if (IsReadLightmapIndex(stream.Version))
			{
				LightmapIndex = IsByteLightIndex(stream.Version) ? stream.ReadByte() : stream.ReadUInt16();
			}

			if(IsReadLightDynamic(stream.Version))
			{
				LightmapIndexDynamic = stream.ReadUInt16();
			}

			if (IsReadMaterialFirst(stream.Version))
			{
				m_materials = stream.ReadArray<PPtr<Material>>();
			}

			if (IsReadTileOffset(stream.Version))
			{
				LightmapTilingOffset.Read(stream);
			}
			if (IsReadTileDynamic(stream.Version))
			{
				LightmapTilingOffsetDynamic.Read(stream);
			}
			
			if (!IsReadMaterialFirst(stream.Version))
			{
				m_materials = stream.ReadArray<PPtr<Material>>();
			}

			if (IsReadSubsetIndices(stream.Version))
			{
				m_subsetIndices = stream.ReadUInt32Array();
			}
			if(IsReadStaticBatchInfo(stream.Version))
			{
				StaticBatchInfo.Read(stream);
			}

			if (IsReadStaticBatchRoot(stream.Version))
			{
				StaticBatchRoot.Read(stream);
			}

			if (IsReadUseLight(stream.Version))
			{
				UseLightProbes = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
			if (IsReadReflectUsage(stream.Version))
			{
				ReflectionProbeUsage = stream.ReadInt32();
			}

			if (IsReadProbeAnchor(stream.Version))
			{
				ProbeAnchor.Read(stream);
			}
			if (IsReadLightOverride(stream.Version))
			{
				LightProbeVolumeOverride.Read(stream);
			}
			if (IsAlignLightProbe(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if (IsReadSortingLayerID(stream.Version))
			{
				SortingLayerID = stream.ReadInt32();
			}
			if (IsReadSortingLayer(stream.Version))
			{
				SortingLayer = stream.ReadInt16();
			}
			if (IsReadSortingOrder(stream.Version))
			{
				SortingOrder = stream.ReadInt16();
			}
			if (IsAlignSortingOrder(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			foreach (PPtr<Material> ptr in Materials)
			{
				if(ptr.IsNull)
				{
					continue;
				}

				Material mat = ptr.FindObject(file);
				if (mat == null)
				{
					if(isLog)
					{
						Logger.Log(LogType.Warning, LogCategory.Export, $"{ToLogString()} m_Materials {ptr.ToLogString(file)} wasn't found ");
					}
				}
				else
				{
					yield return mat;
				}
			}
			if (!StaticBatchRoot.IsNull)
			{
				yield return StaticBatchRoot.GetObject(file);
			}
			if(!ProbeAnchor.IsNull)
			{
				yield return ProbeAnchor.GetObject(file);
			}
			if(!LightProbeVolumeOverride.IsNull)
			{
				yield return LightProbeVolumeOverride.GetObject(file);
			}
		}

		protected void ReadBase(AssetStream stream)
		{
			base.Read(stream);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
#warning TODO: check undefined vars
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_Enabled", Enabled);
			node.Add("m_CastShadows", CastShadows);
			node.Add("m_ReceiveShadows", ReceiveShadows);
			if(IsReadDynamicOccludee(exporter.Version))
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
			node.Add("m_Materials", Materials.ExportYAML(exporter));
			if (IsReadSubsetIndices(exporter.Version))
			{
				StaticBatchInfo staticBatchInfo = new StaticBatchInfo(SubsetIndices);
				node.Add("m_StaticBatchInfo", staticBatchInfo.ExportYAML(exporter));
			}
			else
			{
				node.Add("m_StaticBatchInfo", StaticBatchInfo.ExportYAML(exporter));
			}
			node.Add("m_StaticBatchRoot", StaticBatchRoot.ExportYAML(exporter));
			node.Add("m_ProbeAnchor", ProbeAnchor.ExportYAML(exporter));
			node.Add("m_LightProbeVolumeOverride", LightProbeVolumeOverride.ExportYAML(exporter));
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
			node.Add("m_LightmapParameters", default(PPtr<Object>).ExportYAML(exporter));
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
