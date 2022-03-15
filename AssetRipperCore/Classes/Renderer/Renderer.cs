using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Renderer
{
	public abstract class Renderer : Component, IRenderer
	{
		protected Renderer(AssetInfo assetInfo) : base(assetInfo) { }

		#region Versioning Methods
		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool HasEnabled(UnityVersion version) => version.IsGreaterEqual(1, 5);
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasCastShadows(UnityVersion version) => version.IsGreaterEqual(2);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasDynamicOccludee(UnityVersion version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 2021 and greater
		/// </summary>
		public static bool HasStaticShadowCaster(UnityVersion version) => version.IsGreaterEqual(2021);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasMotionVector(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasRenderingLayerMask(UnityVersion version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasRendererPriority(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool HasLightmapIndex(UnityVersion version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(2, 1))
			{
				if (flags.IsRelease())
				{
					return true;
				}
				// unknown version
				if (version.IsLess(5, 0, 0, UnityVersionType.Final))
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasLightmapIndexDynamic(UnityVersion version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(5))
			{
				if (flags.IsRelease())
				{
					return true;
				}
				// unknown version
				if (version.IsLess(5, 0, 0, UnityVersionType.Final))
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool HasLightmapTilingOffset(UnityVersion version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(2, 1))
			{
				if (flags.IsRelease())
				{
					return true;
				}
				// unknown version
				if (version.IsLess(5, 0, 0, UnityVersionType.Final))
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasLightmapTilingOffsetDynamic(UnityVersion version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(5))
			{
				if (flags.IsRelease())
				{
					return true;
				}
				// unknown version
				if (version.IsLess(5, 0, 0, UnityVersionType.Final))
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 3.0.0 to 5.5.0
		/// </summary>
		public static bool HasSubsetIndices(UnityVersion version) => version.IsGreaterEqual(3) && version.IsLess(5, 5);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasStaticBatchInfo(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasStaticBatchRoot(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 3.5.0 to 5.4.0
		/// </summary>
		public static bool HasUseLight(UnityVersion version) => version.IsGreaterEqual(3, 5) && version.IsLess(5, 4);
		/// <summary>
		/// 5.0.0bx
		/// </summary>
		public static bool HasUseReflectionProbes(UnityVersion version) => version.IsEqual(5, 0, 0, UnityVersionType.Beta);
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool HasReflectUsage(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasRayTracingMode(UnityVersion version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasRayTraceProcedural(UnityVersion version) => version.IsGreaterEqual(2020);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasProbeAnchor(UnityVersion version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasLightOverride(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 3.0.0 and greater and Not Release
		/// </summary>
		public static bool HasScaleInLightmap(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(3);
		/// <summary>
		/// 2019.2 and greater and Not Release
		/// </summary>
		public static bool HasReceiveGI(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(2019, 2);
		/// <summary>
		/// 5.0.0f1 and greater and Not Release
		/// </summary>
		public static bool HasPreserveUVs(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 5.2.3 and greater and Not Release
		/// </summary>
		public static bool HasIgnoreNormalsForChartDetection(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 2, 3);
		/// <summary>
		/// 5.0.0f1 and greater and Not Release
		/// </summary>
		public static bool HasImportantGI(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 5.4.x and Not Release
		/// </summary>
		public static bool HasSelectedWireframeHidden(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsEqual(5, 4);
		/// <summary>
		/// 2017.2 and greater and Not Release
		/// </summary>
		public static bool HasStitchLightmapSeams(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 5.5.0 and greater and Not Release
		/// </summary>
		public static bool HasSelectedEditorRenderState(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.2.3 and greater and Not Release
		/// </summary>
		public static bool HasMinimumChartSize(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 2, 3);
		/// <summary>
		/// 5.0.0 and greater and Not Release 
		/// </summary>
		public static bool HasAutoUVMaxDistance(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0bx and Not Release 
		/// </summary>
		public static bool HasGIBackfaceCull(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsEqual(5, 0, 0, UnityVersionType.Beta);
		/// <summary>
		/// 4.5.0 and greater (Release exlude 5.6.0bx)
		/// </summary>
		public static bool HasSortingLayerID(UnityVersion version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(4, 5))
			{
				if (version.IsEqual(5, 6, 0, UnityVersionType.Beta) && flags.IsRelease())
				{
					return false;
				}
				return true;
			}
			return false;
		}
		/// <summary>
		/// 4.3.x or 5.6.0 and greater
		/// </summary>
		public static bool HasSortingLayer(UnityVersion version) => version.IsEqual(4, 3) || version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasSortingOrder(UnityVersion version) => version.IsGreaterEqual(4, 3);

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsLightmapIndexShort(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		private static bool IsMaterialFirst(UnityVersion version)
		{
			return version.IsLess(3);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool IsReflectUsageFirst(UnityVersion version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.6.0bx
		/// </summary>
		private static bool IsSortingLayerIDFirst(UnityVersion version)
		{
			return version.IsEqual(5, 6, 0, UnityVersionType.Beta);
		}

		/// <summary>
		/// 5.0.0 to 5.3.x (NOTE: unknown version)
		/// </summary>
		private static bool IsAlign1(UnityVersion version) => version.IsGreaterEqual(5) && version.IsLess(5, 4);
		/// <summary>
		/// 5.0.0 and greater (NOTE: unknown version)
		/// </summary>
		private static bool IsAlign2(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsAlign3(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		private static bool IsAlign4(UnityVersion version) => version.IsGreaterEqual(4, 5);
		#endregion

		public string FindMaterialPropertyNameByCRC28(uint crc)
		{
			foreach (PPtr<Material.Material> materialPtr in Materials)
			{
				Material.Material material = materialPtr.FindAsset(SerializedFile);
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

			if (HasEnabled(reader.Version))
			{
				Enabled = reader.ReadBoolean();
			}
			if (IsAlign1(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasCastShadows(reader.Version))
			{
				CastShadows = (ShadowCastingMode)reader.ReadByte();
				ReceiveShadows = reader.ReadByte();
			}
			if (HasDynamicOccludee(reader.Version))
			{
				DynamicOccludee = reader.ReadByte();
			}
			if (HasStaticShadowCaster(reader.Version))
			{
				StaticShadowCaster = reader.ReadByte();
			}
			if (HasMotionVector(reader.Version))
			{
				MotionVectors = (MotionVectorGenerationMode)reader.ReadByte();
				LightProbeUsage = (LightProbeUsage)reader.ReadByte();
			}
			if (HasReflectUsage(reader.Version))
			{
				if (IsReflectUsageFirst(reader.Version))
				{
					ReflectionProbeUsage = (ReflectionProbeUsage)reader.ReadByte();
				}
			}
			if (HasRayTracingMode(reader.Version))
			{
				RayTracingMode = (RayTracingMode)reader.ReadByte();
			}
			if (HasRayTraceProcedural(reader.Version))
			{
				RayTraceProcedural = reader.ReadByte();
			}
			if (IsAlign2(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasRenderingLayerMask(reader.Version))
			{
				RenderingLayerMask = reader.ReadUInt32();
			}
			if (HasRendererPriority(reader.Version))
			{
				RendererPriority = reader.ReadInt32();
			}

			if (HasLightmapIndex(reader.Version, reader.Flags))
			{
				LightmapIndex = IsLightmapIndexShort(reader.Version) ? reader.ReadUInt16() : reader.ReadByte();
			}
			if (HasLightmapIndexDynamic(reader.Version, reader.Flags))
			{
				LightmapIndexDynamic = reader.ReadUInt16();
			}

			if (IsMaterialFirst(reader.Version))
			{
				Materials = reader.ReadAssetArray<PPtr<Material.Material>>();
			}

			if (HasLightmapTilingOffset(reader.Version, reader.Flags))
			{
				LightmapTilingOffset.Read(reader);
			}
			if (HasLightmapTilingOffsetDynamic(reader.Version, reader.Flags))
			{
				LightmapTilingOffsetDynamic.Read(reader);
			}

			if (!IsMaterialFirst(reader.Version))
			{
				Materials = reader.ReadAssetArray<PPtr<Material.Material>>();
			}

			if (HasStaticBatchInfo(reader.Version))
			{
				StaticBatchInfo.Read(reader);
			}
			else if (HasSubsetIndices(reader.Version))
			{
				SubsetIndices = reader.ReadUInt32Array();
			}
			if (HasStaticBatchRoot(reader.Version))
			{
				StaticBatchRoot.Read(reader);
			}

			if (HasUseLight(reader.Version))
			{
				bool UseLightProbes = reader.ReadBoolean();
				LightProbeUsage = UseLightProbes ? LightProbeUsage.BlendProbes : LightProbeUsage.Off;
			}
			if (HasUseReflectionProbes(reader.Version))
			{
				bool UseReflectionProbes = reader.ReadBoolean();
				ReflectionProbeUsage = UseReflectionProbes ? ReflectionProbeUsage.Simple : ReflectionProbeUsage.Off;
			}
			if (HasUseLight(reader.Version))
			{
				reader.AlignStream();
			}
			if (HasReflectUsage(reader.Version))
			{
				if (!IsReflectUsageFirst(reader.Version))
				{
					ReflectionProbeUsage = (ReflectionProbeUsage)reader.ReadInt32();
				}
			}

			if (HasProbeAnchor(reader.Version))
			{
				ProbeAnchor.Read(reader);
			}
			if (HasLightOverride(reader.Version))
			{
				LightProbeVolumeOverride.Read(reader);
			}
			if (HasSortingLayerID(reader.Version, reader.Flags))
			{
				if (IsSortingLayerIDFirst(reader.Version))
				{
					SortingLayerID = reader.ReadInt32();
				}
			}
			if (IsAlign3(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasSortingLayerID(reader.Version, reader.Flags))
			{
				if (!IsSortingLayerIDFirst(reader.Version))
				{
					SortingLayerID = reader.ReadInt32();
				}
			}
			if (HasSortingLayer(reader.Version))
			{
				SortingLayer = reader.ReadInt16();
			}
			if (HasSortingOrder(reader.Version))
			{
				SortingOrder = reader.ReadInt16();
			}
			if (IsAlign4(reader.Version))
			{
				reader.AlignStream();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Materials, MaterialsName))
			{
				yield return asset;
			}
			yield return context.FetchDependency(StaticBatchRoot, StaticBatchRootName);
			yield return context.FetchDependency(ProbeAnchor, ProbeAnchorName);
			yield return context.FetchDependency(LightProbeVolumeOverride, LightProbeVolumeOverrideName);
		}

		protected void ReadBase(AssetReader reader)
		{
			base.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(EnabledName, GetEnabled(container.Version));
			node.Add(CastShadowsName, (byte)GetCastShadows(container.Version));
			node.Add(ReceiveShadowsName, GetReceiveShadows(container.Version));
			node.Add(DynamicOccludeeName, GetDynamicOccludee(container.Version));
			if (HasStaticShadowCaster(container.ExportVersion))
			{
				node.Add(StaticShadowCasterName, StaticShadowCaster);
			}
			node.Add(MotionVectorsName, (byte)GetMotionVectors(container.Version));
			node.Add(LightProbeUsageName, (byte)LightProbeUsage);
			node.Add(ReflectionProbeUsageName, (byte)GetReflectionProbeUsage(container.Version));
			if (HasRayTracingMode(container.ExportVersion))
			{
				node.Add(RayTracingModeName, (byte)RayTracingMode);
			}
			if (HasRayTraceProcedural(container.ExportVersion))
			{
				node.Add(RayTraceProceduralName, RayTraceProcedural);
			}
			if (HasRenderingLayerMask(container.ExportVersion))
			{
				node.Add(RenderingLayerMaskName, GetRenderingLayerMask(container.Version));
			}
			if (HasRendererPriority(container.ExportVersion))
			{
				node.Add(RendererPriorityName, RendererPriority);
			}
			node.Add(MaterialsName, Materials.ExportYAML(container));
			node.Add(StaticBatchInfoName, GetStaticBatchInfo(container.Version).ExportYAML(container));
			node.Add(StaticBatchRootName, StaticBatchRoot.ExportYAML(container));
			node.Add(ProbeAnchorName, ProbeAnchor.ExportYAML(container));
			node.Add(LightProbeVolumeOverrideName, LightProbeVolumeOverride.ExportYAML(container));
			node.Add(ScaleInLightmapName, GetScaleInLightmap(container.Version, container.Flags));
			if (HasReceiveGI(container.ExportVersion, container.ExportFlags))
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

		private bool GetEnabled(UnityVersion version)
		{
			return HasEnabled(version) ? Enabled : true;
		}
		private ShadowCastingMode GetCastShadows(UnityVersion version)
		{
			return HasCastShadows(version) ? CastShadows : ShadowCastingMode.On;
		}
		private byte GetReceiveShadows(UnityVersion version)
		{
			return HasCastShadows(version) ? ReceiveShadows : (byte)1;
		}
		private int GetDynamicOccludee(UnityVersion version)
		{
			return HasDynamicOccludee(version) ? DynamicOccludee : 1;
		}
		private MotionVectorGenerationMode GetMotionVectors(UnityVersion version)
		{
			return HasMotionVector(version) ? MotionVectors : MotionVectorGenerationMode.Object;
		}
		private ReflectionProbeUsage GetReflectionProbeUsage(UnityVersion version)
		{
			return HasReflectUsage(version) ? ReflectionProbeUsage : ReflectionProbeUsage.BlendProbes;
		}
		private uint GetRenderingLayerMask(UnityVersion version)
		{
			return HasRenderingLayerMask(version) ? RenderingLayerMask : 1;
		}
		public IStaticBatchInfo GetStaticBatchInfo(UnityVersion version)
		{
			if (HasStaticBatchInfo(version))
			{
				return StaticBatchInfo;
			}
			else if (HasSubsetIndices(version))
			{
				return new StaticBatchInfo(SubsetIndices);
			}
			return new StaticBatchInfo();
		}
		private float GetScaleInLightmap(UnityVersion version, TransferInstructionFlags flags)
		{
			return 1.0f;
		}
		private ReceiveGI GetReceiveGI(UnityVersion version, TransferInstructionFlags flags)
		{
			return ReceiveGI.Lightmaps;
		}
		private bool GetPreserveUVs(UnityVersion version, TransferInstructionFlags flags)
		{
			return false;
		}
		private bool GetIgnoreNormalsForChartDetection(UnityVersion version, TransferInstructionFlags flags)
		{
			return false;
		}
		private bool GetImportantGI(UnityVersion version, TransferInstructionFlags flags)
		{
			return false;
		}
		private bool GetStitchLightmapSeams(UnityVersion version, TransferInstructionFlags flags)
		{
			return false;
		}
		private EditorSelectedRenderState GetSelectedEditorRenderState(UnityVersion version, TransferInstructionFlags flags)
		{
			return (EditorSelectedRenderState)3;
		}
		private int GetMinimumChartSize(UnityVersion version, TransferInstructionFlags flags)
		{
			return 4;
		}
		private float GetAutoUVMaxDistance(UnityVersion version, TransferInstructionFlags flags)
		{
			return 0.5f;
		}
		private float GetAutoUVMaxAngle(UnityVersion version, TransferInstructionFlags flags)
		{
			return 89.0f;
		}
		private PPtr<LightmapParameters> GetLightmapParameters()
		{
			return new();
		}

		public bool Enabled { get; set; }
		public ShadowCastingMode CastShadows { get; set; }
		public byte ReceiveShadows { get; set; }
		public byte DynamicOccludee { get; set; }
		public byte StaticShadowCaster { get; set; }
		public MotionVectorGenerationMode MotionVectors { get; set; }
		public bool UseLightProbes => LightProbeUsage != LightProbeUsage.Off;
		public LightProbeUsage LightProbeUsage { get; set; }
		public bool UseReflectionProbes => ReflectionProbeUsage != ReflectionProbeUsage.Off;
		public ReflectionProbeUsage ReflectionProbeUsage { get; set; }
		public RayTracingMode RayTracingMode { get; set; }
		public byte RayTraceProcedural { get; set; }
		public uint RenderingLayerMask { get; set; }
		public int RendererPriority { get; set; }
		public ushort LightmapIndex { get; set; }
		public ushort LightmapIndexDynamic { get; set; }
		public PPtr<Material.Material>[] Materials { get; set; }
		public uint[] SubsetIndices { get; set; }
		public int SortingLayerID { get; set; }
		public short SortingLayer { get; set; }
		public short SortingOrder { get; set; }

		public const string EnabledName = "m_Enabled";
		public const string CastShadowsName = "m_CastShadows";
		public const string ReceiveShadowsName = "m_ReceiveShadows";
		public const string DynamicOccludeeName = "m_DynamicOccludee";
		public const string StaticShadowCasterName = "m_StaticShadowCaster";
		public const string MotionVectorsName = "m_MotionVectors";
		public const string UseLightProbesName = "m_UseLightProbes";
		public const string LightProbeUsageName = "m_LightProbeUsage";
		public const string ReflectionProbeUsageName = "m_ReflectionProbeUsage";
		public const string RayTracingModeName = "m_RayTracingMode";
		public const string RayTraceProceduralName = "m_RayTraceProcedural";
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

		public Vector4f LightmapTilingOffset { get; set; } = new();
		public Vector4f LightmapTilingOffsetDynamic { get; set; } = new();
		public IStaticBatchInfo StaticBatchInfo { get; } = new StaticBatchInfo();
		public PPtr<Transform> StaticBatchRoot { get; set; } = new();
		/// <summary>
		/// LightProbeAnchor previously
		/// </summary>
		public PPtr<Transform> ProbeAnchor { get; set; } = new();
		public PPtr<GameObject.GameObject> LightProbeVolumeOverride { get; set; } = new();
	}
}
