using System.Collections.Generic;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Classes.ReflectionProbes;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class ReflectionProbe : Behaviour
	{
		public ReflectionProbe(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			// NOTE: unknown version
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Final))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasRefreshMode(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasImportance(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasBlendDistance(Version version) => version.IsGreaterEqual(5, 2);
		/// <summary>
		/// Less than 5.0.0f1 (NOTE: unknown version)
		/// </summary>
		public static bool HasBakedRenderPassCount(Version version) => version.IsLess(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasBoxProjection(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasCustomBakedTexture(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// (Less than 5.0.0) or (5.0.0 and greater and Release)
		/// </summary>
		public static bool HasBakedTexture(Version version, TransferInstructionFlags flags)
		{
			// NOTE: unknown version
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Final))
			{
				return flags.IsRelease();
			}
			return true;
		}

		/// <summary>
		/// 5.0.0f1 to 5.4.0 exclusive (NOTE: unknown version)
		/// </summary>
		private static bool HasImportanceFirst(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final) && version.IsLess(5, 4);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Type = (ReflectionProbeType)reader.ReadInt32();
			Mode = (ReflectionProbeMode)reader.ReadInt32();
			if (HasRefreshMode(reader.Version))
			{
				RefreshMode = (ReflectionProbeRefreshMode)reader.ReadInt32();
				TimeSlicingMode = (ReflectionProbeTimeSlicingMode)reader.ReadInt32();
			}
			Resolution = reader.ReadInt32();
			UpdateFrequency = reader.ReadInt32();
			if (HasImportance(reader.Version))
			{
				if (HasImportanceFirst(reader.Version))
				{
					Importance = reader.ReadInt16();
				}
			}
			BoxSize.Read(reader);
			BoxOffset.Read(reader);
			NearClip = reader.ReadSingle();
			FarClip = reader.ReadSingle();
			ShadowDistance = reader.ReadSingle();
			ClearFlags = (ReflectionProbeClearFlags)reader.ReadUInt32();
			BackGroundColor.Read(reader);
			CullingMask.Read(reader);
			IntensityMultiplier = reader.ReadSingle();
			if (HasBlendDistance(reader.Version))
			{
				BlendDistance = reader.ReadSingle();
			}
			if (HasBakedRenderPassCount(reader.Version))
			{
				BakedRenderPassCount = reader.ReadUInt32();
				UseMipMap = reader.ReadBoolean();
			}
			HDR = reader.ReadBoolean();
			if (HasBoxProjection(reader.Version))
			{
				BoxProjection = reader.ReadBoolean();
				RenderDynamicObjects = reader.ReadBoolean();
				UseOcclusionCulling = reader.ReadBoolean();
			}
			if (HasImportance(reader.Version))
			{
				if (!HasImportanceFirst(reader.Version))
				{
					Importance = reader.ReadInt16();
				}
			}
			reader.AlignStream();
			
			if (HasCustomBakedTexture(reader.Version))
			{
				CustomBakedTexture.Read(reader);
			}
			if (HasBakedTexture(reader.Version, reader.Flags))
			{
				BakedTextureTexture.Read(reader);
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(CustomBakedTexture, CustomBakedTextureName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TypeName, (int)Type);
			node.Add(ModeName, (int)Mode);
			node.Add(RefreshModeName, (int)RefreshMode);
			node.Add(TimeSlicingModeName, (int)TimeSlicingMode);
			node.Add(ResolutionName, Resolution);
			node.Add(UpdateFrequencyName, UpdateFrequency);
			node.Add(BoxSizeName, BoxSize.ExportYAML(container));
			node.Add(BoxOffsetName, BoxOffset.ExportYAML(container));
			node.Add(NearClipName, NearClip);
			node.Add(FarClipName, FarClip);
			node.Add(ShadowDistanceName, ShadowDistance);
			node.Add(ClearFlagsName, (uint)ClearFlags);
			node.Add(BackGroundColorName, BackGroundColor.ExportYAML(container));
			node.Add(CullingMaskName, CullingMask.ExportYAML(container));
			node.Add(IntensityMultiplierName, IntensityMultiplier);
			node.Add(BlendDistanceName, BlendDistance);
			node.Add(HDRName, HDR);
			node.Add(BoxProjectionName, BoxProjection);
			node.Add(RenderDynamicObjectsName, RenderDynamicObjects);
			node.Add(UseOcclusionCullingName, UseOcclusionCulling);
			node.Add(ImportanceName, Importance);
			node.Add(CustomBakedTextureName, CustomBakedTexture.ExportYAML(container));
			if (HasBakedTexture(container.ExportVersion, container.ExportFlags))
			{
				node.Add(BakedTextureName, BakedTextureTexture.ExportYAML(container));
			}
			return node;
		}

		public ReflectionProbeType Type { get; set; }
		public ReflectionProbeMode Mode { get; set; }
		public ReflectionProbeRefreshMode RefreshMode { get; set; }
		public ReflectionProbeTimeSlicingMode TimeSlicingMode { get; set; }
		public int Resolution { get; set; }
		public int UpdateFrequency { get; set; }
		public float NearClip { get; set; }
		public float FarClip { get; set; }
		public float ShadowDistance { get; set; }
		public ReflectionProbeClearFlags ClearFlags { get; set; }
		public float IntensityMultiplier { get; set; }
		public float BlendDistance { get; set; }
		public uint BakedRenderPassCount { get; set; }
		public bool UseMipMap { get; set; }
		public bool HDR { get; set; }
		public bool BoxProjection { get; set; }
		public bool RenderDynamicObjects { get; set; }
		public bool UseOcclusionCulling { get; set; }
		public short Importance { get; set; }

		public const string TypeName = "m_Type";
		public const string ModeName = "m_Mode";
		public const string RefreshModeName = "m_RefreshMode";
		public const string TimeSlicingModeName = "m_TimeSlicingMode";
		public const string ResolutionName = "m_Resolution";
		public const string UpdateFrequencyName = "m_UpdateFrequency";
		public const string BoxSizeName = "m_BoxSize";
		public const string BoxOffsetName = "m_BoxOffset";
		public const string NearClipName = "m_NearClip";
		public const string FarClipName = "m_FarClip";
		public const string ShadowDistanceName = "m_ShadowDistance";
		public const string ClearFlagsName = "m_ClearFlags";
		public const string BackGroundColorName = "m_BackGroundColor";
		public const string CullingMaskName = "m_CullingMask";
		public const string IntensityMultiplierName = "m_IntensityMultiplier";
		public const string BlendDistanceName = "m_BlendDistance";
		public const string HDRName = "m_HDR";
		public const string BoxProjectionName = "m_BoxProjection";
		public const string RenderDynamicObjectsName = "m_RenderDynamicObjects";
		public const string UseOcclusionCullingName = "m_UseOcclusionCulling";
		public const string ImportanceName = "m_Importance";
		public const string CustomBakedTextureName = "m_CustomBakedTexture";
		public const string BakedTextureName = "m_BakedTexture";

		public Vector3f BoxSize;
		public Vector3f BoxOffset;
		public ColorRGBAf BackGroundColor;
		public BitField CullingMask;
		public PPtr<Texture> CustomBakedTexture;
		public PPtr<Texture> BakedTextureTexture;
	}
}
