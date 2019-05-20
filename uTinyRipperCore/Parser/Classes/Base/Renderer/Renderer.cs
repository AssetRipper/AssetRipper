using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.MeshRenderers;
using uTinyRipper.Classes.Renderers;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
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
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadRendererPriority(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool IsReadLightmapIndex(Version version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(2, 1))
			{
				if (flags.IsRelease())
				{
					return true;
				}
				// unknown version
				if (version.IsLess(5, 0, 0, VersionType.Final))
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLightmapIndexDynamic(Version version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(5))
			{
				if (flags.IsRelease())
				{
					return true;
				}
				// unknown version
				if (version.IsLess(5, 0, 0, VersionType.Final))
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool IsReadLightmapTilingOffset(Version version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(2, 1))
			{
				if (flags.IsRelease())
				{
					return true;
				}
				// unknown version
				if (version.IsLess(5, 0, 0, VersionType.Final))
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLightmapTilingOffsetDynamic(Version version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(5))
			{
				if (flags.IsRelease())
				{
					return true;
				}
				// unknown version
				if (version.IsLess(5, 0, 0, VersionType.Final))
				{
					return true;
				}
			}
			return false;
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
		/// 3.0.0 and greater
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
		/// 5.0.0bx
		/// </summary>
		public static bool IsReadUseReflectionProbes(Version version)
		{
			return version.IsEqual(5, 0, 0, VersionType.Beta);
		}
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadReflectUsage(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
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
		/// 3.0.0 and greater and Not Release
		/// </summary>
		public static bool IsReadScaleInLightmap(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 2019.2 and greater and Not Release
		/// </summary>
		public static bool IsReadReceiveGI(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2019, 2);
		}
		/// <summary>
		/// 5.0.0f1 and greater and Not Release
		/// </summary>
		public static bool IsReadPreserveUVs(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.2.3 and greater and Not Release
		/// </summary>
		public static bool IsReadIgnoreNormalsForChartDetection(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5, 2, 3);
		}
		/// <summary>
		/// 5.0.0f1 and greater and Not Release
		/// </summary>
		public static bool IsReadImportantGI(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.4.x and Not Release
		/// </summary>
		public static bool IsReadSelectedWireframeHidden(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsEqual(5, 4);
		}
		/// <summary>
		/// 2017.2 and greater and Not Release
		/// </summary>
		public static bool IsReadStitchLightmapSeams(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 5.5.0 and greater and Not Release
		/// </summary>
		public static bool IsReadSelectedEditorRenderState(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 5.2.3 and greater and Not Release
		/// </summary>
		public static bool IsReadMinimumChartSize(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5, 2, 3);
		}
		/// <summary>
		/// 5.0.0 and greater and Not Release 
		/// </summary>
		public static bool IsReadAutoUVMaxDistance(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0bx and Not Release 
		/// </summary>
		public static bool IsReadGIBackfaceCull(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsEqual(5, 0, 0, VersionType.Beta);
		}
		/// <summary>
		/// 4.5.0 and greater but exluding 5.6.0b
		/// </summary>
		public static bool IsReadSortingLayerID(Version version)
		{
			if (version.IsGreaterEqual(4, 5))
			{
				if (version.IsLess(5, 6))
				{
					return true;
				}
				if (version.IsGreater(5, 6, 0, VersionType.Beta))
				{
					return true;
				}
			}
			return false;
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
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsLightmapIndexShort(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		private static bool IsReadMaterialFirst(Version version)
		{
			return version.IsLess(3);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool IsReadReflectUsageFirst(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsLightmapTilingOffsetV4(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}

		/// <summary>
		/// 5.0.0 to 5.3.x
		/// </summary>
		private static bool IsAlign1(Version version)
		{
			// unknown lower version
			return version.IsGreaterEqual(5) && version.IsLess(5, 4);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsAlign2(Version version)
		{
			// unknown lower version
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsAlign3(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		private static bool IsAlign4(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}

		public string FindMaterialPropertyNameByCRC28(uint crc)
		{
			foreach (PPtr<Material> materialPtr in m_materials)
			{
				Material material = materialPtr.FindAsset(File);
				if (material == null)
				{
					continue;
				}
				string property = material.FindPropertyNameByCRC28(crc);
				if (property == null)
				{
					continue;
				}

				return property;
			}
			return null;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Enabled = reader.ReadBoolean();
			if (IsAlign1(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			CastShadows = (ShadowCastingMode)reader.ReadByte();
			ReceiveShadows = reader.ReadByte();
			if (IsReadDynamicOccludee(reader.Version))
			{
				DynamicOccludee = reader.ReadByte();
			}
			if (IsReadMotionVector(reader.Version))
			{
				MotionVectors = (MotionVectorGenerationMode)reader.ReadByte();
				LightProbeUsage = (LightProbeUsage)reader.ReadByte();
			}
			if (IsReadReflectUsage(reader.Version))
			{
				if (IsReadReflectUsageFirst(reader.Version))
				{
					ReflectionProbeUsage = (ReflectionProbeUsage)reader.ReadByte();
				}
			}
			if (IsAlign2(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadRenderingLayerMask(reader.Version))
			{
				RenderingLayerMask = reader.ReadUInt32();
			}
			if (IsReadRendererPriority(reader.Version))
			{
				RendererPriority = reader.ReadInt32();
			}

			if (IsReadLightmapIndex(reader.Version, reader.Flags))
			{
				LightmapIndex = IsLightmapIndexShort(reader.Version) ? reader.ReadUInt16() : reader.ReadByte();
			}
			if (IsReadLightmapIndexDynamic(reader.Version, reader.Flags))
			{
				LightmapIndexDynamic = reader.ReadUInt16();
			}

			if (IsReadMaterialFirst(reader.Version))
			{
				m_materials = reader.ReadAssetArray<PPtr<Material>>();
			}

			if (IsReadLightmapTilingOffset(reader.Version, reader.Flags))
			{
				if (IsLightmapTilingOffsetV4(reader.Version))
				{
					LightmapTilingOffset.Read(reader);
				}
				else
				{
					LightmapTilingOffset.Read3(reader);
				}
			}
			if (IsReadLightmapTilingOffsetDynamic(reader.Version, reader.Flags))
			{
				LightmapTilingOffsetDynamic.Read(reader);
			}

			if (!IsReadMaterialFirst(reader.Version))
			{
				m_materials = reader.ReadAssetArray<PPtr<Material>>();
			}

			if (IsReadStaticBatchInfo(reader.Version))
			{
				StaticBatchInfo.Read(reader);
			}
			else if (IsReadSubsetIndices(reader.Version))
			{
				m_subsetIndices = reader.ReadUInt32Array();
			}
			if (IsReadStaticBatchRoot(reader.Version))
			{
				StaticBatchRoot.Read(reader);
			}

			if (IsReadUseLight(reader.Version))
			{
				bool UseLightProbes = reader.ReadBoolean();
				LightProbeUsage = UseLightProbes ? LightProbeUsage.BlendProbes : LightProbeUsage.Off;
			}
			if (IsReadUseReflectionProbes(reader.Version))
			{
				bool UseReflectionProbes = reader.ReadBoolean();
				ReflectionProbeUsage = UseReflectionProbes ? ReflectionProbeUsage.Simple : ReflectionProbeUsage.Off;
			}
			if (IsReadUseLight(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadReflectUsage(reader.Version))
			{
				if (!IsReadReflectUsageFirst(reader.Version))
				{
					ReflectionProbeUsage = (ReflectionProbeUsage)reader.ReadInt32();
				}
			}

			if (IsReadProbeAnchor(reader.Version))
			{
				ProbeAnchor.Read(reader);
			}
			if (IsReadLightOverride(reader.Version))
			{
				LightProbeVolumeOverride.Read(reader);
			}
#if UNIVERSAL
			if (IsReadScaleInLightmap(reader.Version, reader.Flags))
			{
				ScaleInLightmap = reader.ReadSingle();
			}
			if (IsReadReceiveGI(reader.Version, reader.Flags))
			{
				ReceiveGI = (ReceiveGI)reader.ReadInt32();
			}
			if (IsReadPreserveUVs(reader.Version, reader.Flags))
			{
				PreserveUVs = reader.ReadBoolean();
			}
			if (IsReadIgnoreNormalsForChartDetection(reader.Version, reader.Flags))
			{
				IgnoreNormalsForChartDetection = reader.ReadBoolean();
			}
			if (IsReadImportantGI(reader.Version, reader.Flags))
			{
				ImportantGI = reader.ReadBoolean();
			}
			if (IsReadSelectedWireframeHidden(reader.Version, reader.Flags))
			{
				SelectedWireframeHidden = reader.ReadBoolean();
			}
			if (IsReadStitchLightmapSeams(reader.Version, reader.Flags))
			{
				StitchLightmapSeams = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadSelectedEditorRenderState(reader.Version, reader.Flags))
			{
				SelectedEditorRenderState = (EditorSelectedRenderState)reader.ReadInt32();
			}
			if (IsReadMinimumChartSize(reader.Version, reader.Flags))
			{
				MinimumChartSize = reader.ReadInt32();
			}
			if (IsReadAutoUVMaxDistance(reader.Version, reader.Flags))
			{
				AutoUVMaxDistance = reader.ReadSingle();
				AutoUVMaxAngle = reader.ReadSingle();
				LightmapParameters.Read(reader);
			}
			if (IsReadGIBackfaceCull(reader.Version, reader.Flags))
			{
				GIBackfaceCull = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
#endif
			if (IsAlign3(reader.Version))
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
			if (IsAlign4(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			foreach (PPtr<Material> material in Materials)
			{
				yield return material.FetchDependency(file, isLog, ToLogString, "m_Materials");
			}
			if (!StaticBatchRoot.IsNull)
			{
				yield return StaticBatchRoot.GetAsset(file);
			}
			if (!ProbeAnchor.IsNull)
			{
				yield return ProbeAnchor.GetAsset(file);
			}
			if (!LightProbeVolumeOverride.IsNull)
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
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(EnabledName, Enabled);
			node.Add(CastShadowsName, (byte)CastShadows);
			node.Add(ReceiveShadowsName, ReceiveShadows);
			node.Add(DynamicOccludeeName, GetDynamicOccludee(container.Version));
			node.Add(MotionVectorsName, (byte)GetMotionVectors(container.Version));
			node.Add(LightProbeUsageName, (byte)LightProbeUsage);
			node.Add(ReflectionProbeUsageName, (byte)GetReflectionProbeUsage(container.Version));
			if (IsReadRenderingLayerMask(container.ExportVersion))
			{
				node.Add(RenderingLayerMaskName, GetRenderingLayerMask(container.Version));
			}
			if (IsReadRendererPriority(container.ExportVersion))
			{
				node.Add(RendererPriorityName, RendererPriority);
			}
			node.Add(MaterialsName, Materials.ExportYAML(container));
			node.Add(StaticBatchInfoName, GetStaticBatchInfo(container.Version).ExportYAML(container));
			node.Add(StaticBatchRootName, StaticBatchRoot.ExportYAML(container));
			node.Add(ProbeAnchorName, ProbeAnchor.ExportYAML(container));
			node.Add(LightProbeVolumeOverrideName, LightProbeVolumeOverride.ExportYAML(container));
			node.Add(ScaleInLightmapName, GetScaleInLightmap(container.Version, container.Flags));
			if (IsReadReceiveGI(container.ExportVersion, container.ExportFlags))
			{
				node.Add(ReceiveGIName, (int)GetReceiveGI(container.Version, container.Flags));
			}
			node.Add(PreserveUVsName, GetPreserveUVs(container.Version, container.Flags));
			node.Add(IgnoreNormalsForChartDetectionName, GetIgnoreNormalsForChartDetection(container.Version, container.Flags));
			node.Add(ImportantGIName, GetImportantGI(container.Version, container.Flags));
			node.Add(StitchLightmapSeamsName, GetStitchLightmapSeams(container.Version, container.Flags));
			node.Add(SelectedEditorRenderStateName, (int)GetSelectedEditorRenderState(container.Version, container.Flags));
			node.Add(MinimumChartSizeName, GetMinimumChartSize(container.Version, container.Flags));
			node.Add(AutoUVMaxDistanceName, GetAutoUVMaxDistance(container.Version, container.Flags));
			node.Add(AutoUVMaxAngleName, GetAutoUVMaxAngle(container.Version, container.Flags));
			node.Add(LightmapParametersName, GetLightmapParameters().ExportYAML(container));
			node.Add(SortingLayerIDName, SortingLayerID);
			node.Add(SortingLayerName, SortingLayer);
			node.Add(SortingOrderName, SortingOrder);
			return node;
		}

		private int GetDynamicOccludee(Version version)
		{
			return IsReadDynamicOccludee(version) ? DynamicOccludee : 1;
		}
		private MotionVectorGenerationMode GetMotionVectors(Version version)
		{
			return IsReadMotionVector(version) ? MotionVectors : MotionVectorGenerationMode.Object;
		}
		private ReflectionProbeUsage GetReflectionProbeUsage(Version version)
		{
			return IsReadReflectUsage(version) ? ReflectionProbeUsage : ReflectionProbeUsage.BlendProbes;
		}
		private uint GetRenderingLayerMask(Version version)
		{
			return IsReadRenderingLayerMask(version) ? RenderingLayerMask : 1;
		}
		private StaticBatchInfo GetStaticBatchInfo(Version version)
		{
			if (IsReadStaticBatchInfo(version))
			{
				return StaticBatchInfo;
			}
			else if (IsReadSubsetIndices(version))
			{
				return new StaticBatchInfo(SubsetIndices);
			}
			return default; 
		}
		private float GetScaleInLightmap(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadScaleInLightmap(version, flags))
			{
				return ScaleInLightmap;
			}
#endif
			return 1.0f;
		}
		private ReceiveGI GetReceiveGI(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadReceiveGI(version, flags))
			{
				return ReceiveGI;
			}
#endif
			return ReceiveGI.Lightmaps;
		}
		private bool GetPreserveUVs(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadPreserveUVs(version, flags))
			{
				return PreserveUVs;
			}
#endif
			return false;
		}
		private bool GetIgnoreNormalsForChartDetection(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadIgnoreNormalsForChartDetection(version, flags))
			{
				return IgnoreNormalsForChartDetection;
			}
#endif
			return false;
		}
		private bool GetImportantGI(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadImportantGI(version, flags))
			{
				return ImportantGI;
			}
#endif
			return false;
		}
		private bool GetStitchLightmapSeams(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadStitchLightmapSeams(version, flags))
			{
				return StitchLightmapSeams;
			}
#endif
			return false;
		}
		private EditorSelectedRenderState GetSelectedEditorRenderState(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadSelectedEditorRenderState(version, flags))
			{
				return SelectedEditorRenderState;
			}
#endif
			return (EditorSelectedRenderState)3;
		}
		private int GetMinimumChartSize(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadMinimumChartSize(version, flags))
			{
				return MinimumChartSize;
			}
#endif
			return 4;
		}
		private float GetAutoUVMaxDistance(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadAutoUVMaxDistance(version, flags))
			{
				return AutoUVMaxDistance;
			}
#endif
			return 0.5f;
		}
		private float GetAutoUVMaxAngle(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadAutoUVMaxDistance(version, flags))
			{
				return AutoUVMaxAngle;
			}
#endif
			return 89.0f;
		}
		private PPtr<LightmapParameters> GetLightmapParameters()
		{
#if UNIVERSAL
			return LightmapParameters;
#else
			return default;
#endif
		}

		public bool Enabled { get; private set; }
		public ShadowCastingMode CastShadows { get; private set; }
		public byte ReceiveShadows { get; private set; }
		public byte DynamicOccludee { get; private set; }
		public MotionVectorGenerationMode MotionVectors { get; private set; }
		public bool UseLightProbes => LightProbeUsage != LightProbeUsage.Off;
		public LightProbeUsage LightProbeUsage { get; private set; }
		public bool UseReflectionProbes => ReflectionProbeUsage != ReflectionProbeUsage.Off;
		public ReflectionProbeUsage ReflectionProbeUsage { get; private set; }
		public uint RenderingLayerMask { get; private set; }
		public int RendererPriority { get; private set; }
		public ushort LightmapIndex { get; private set; }
		public ushort LightmapIndexDynamic { get; private set; }
		public IReadOnlyList<PPtr<Material>> Materials => m_materials;
		public IReadOnlyList<uint> SubsetIndices => m_subsetIndices;
#if UNIVERSAL
		public float ScaleInLightmap { get; private set; }
		public ReceiveGI ReceiveGI { get; private set; }
		public bool PreserveUVs { get; private set; }
		public bool IgnoreNormalsForChartDetection { get; private set; }
		public bool ImportantGI { get; private set; }
		public bool SelectedWireframeHidden { get; private set; }
		public bool StitchLightmapSeams { get; private set; }

		public EditorSelectedRenderState SelectedEditorRenderState { get; private set; }
		public int MinimumChartSize { get; private set; }
		public float AutoUVMaxDistance { get; private set; }
		public float AutoUVMaxAngle { get; private set; }
		public bool GIBackfaceCull { get; private set; }
#endif
		public int SortingLayerID { get; private set; }
		public short SortingLayer { get; private set; }
		public short SortingOrder { get; private set; }

		public const string EnabledName = "m_Enabled";
		public const string CastShadowsName = "m_CastShadows";
		public const string ReceiveShadowsName = "m_ReceiveShadows";
		public const string DynamicOccludeeName = "m_DynamicOccludee";
		public const string MotionVectorsName = "m_MotionVectors";
		public const string UseLightProbesName = "m_UseLightProbes";
		public const string LightProbeUsageName = "m_LightProbeUsage";
		public const string ReflectionProbeUsageName = "m_ReflectionProbeUsage";
		public const string RenderingLayerMaskName = "m_RenderingLayerMask";
		public const string RendererPriorityName = "m_RendererPriority";
		public const string MaterialsName = "m_Materials";
		public const string StaticBatchInfoName = "m_StaticBatchInfo";
		public const string StaticBatchRootName = "m_StaticBatchRoot";
		public const string ProbeAnchorName = "m_ProbeAnchor";
		public const string LightProbeVolumeOverrideName = "m_LightProbeVolumeOverride";
		public const string ScaleInLightmapName = "m_ScaleInLightmap";
		public const string ReceiveGIName = "m_ReceiveGI";
		public const string PreserveUVsName = "m_PreserveUVs";
		public const string IgnoreNormalsForChartDetectionName = "m_IgnoreNormalsForChartDetection";
		public const string ImportantGIName = "m_ImportantGI";
		public const string SelectedWireframeHiddenName = "m_SelectedWireframeHidden";
		public const string StitchLightmapSeamsName = "m_StitchLightmapSeams";
		public const string SelectedEditorRenderStateName = "m_SelectedEditorRenderState";
		public const string MinimumChartSizeName = "m_MinimumChartSize";
		public const string AutoUVMaxDistanceName = "m_AutoUVMaxDistance";
		public const string AutoUVMaxAngleName = "m_AutoUVMaxAngle";
		public const string LightmapParametersName = "m_LightmapParameters";
		public const string SortingLayerIDName = "m_SortingLayerID";
		public const string SortingLayerName = "m_SortingLayer";
		public const string SortingOrderName = "m_SortingOrder";

		public Vector4f LightmapTilingOffset;
		public Vector4f LightmapTilingOffsetDynamic;
		public StaticBatchInfo StaticBatchInfo;
		public PPtr<Transform> StaticBatchRoot;
		/// <summary>
		/// LightProbeAnchor previously
		/// </summary>
		public PPtr<Transform> ProbeAnchor;
		public PPtr<GameObject> LightProbeVolumeOverride;
#if UNIVERSAL
		/// <summary>
		/// EnlightenSystemBuildParameters previously
		/// </summary>
		public PPtr<LightmapParameters> LightmapParameters;
#endif

		private PPtr<Material>[] m_materials;
		private uint[] m_subsetIndices;
	}
}
