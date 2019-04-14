using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.ReflectionProbes;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class ReflectionProbe : Behaviour
	{
		public ReflectionProbe(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadRefreshMode(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadImportance(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool IsReadBlendDistance(Version version)
		{
			return version.IsGreaterEqual(5, 2);
		}
		/// <summary>
		/// Less than 5.0.0f1
		/// </summary>
		public static bool IsReadBakedRenderPassCount(Version version)
		{
			// unknown version
			return version.IsLess(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadBoxProjection(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadCustomBakedTexture(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		public static bool IsReadBakedTexture(Version version, TransferInstructionFlags flags)
		{
			// unknown version
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Final))
			{
				return flags.IsRelease();
			}
			return true;
		}

		/// <summary>
		/// 5.0.0f1 to 5.4.0 exclusive
		/// </summary>
		private static bool IsReadImportanceFirst(Version version)
		{
			// unknown start version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final) && version.IsLess(5, 4);
		}

		private static int GetSerializedVersion(Version version)
		{
			// unknown version
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Final))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Type = (ReflectionProbeType)reader.ReadInt32();
			Mode = (ReflectionProbeMode)reader.ReadInt32();
			if (IsReadRefreshMode(reader.Version))
			{
				RefreshMode = (ReflectionProbeRefreshMode)reader.ReadInt32();
				TimeSlicingMode = (ReflectionProbeTimeSlicingMode)reader.ReadInt32();
			}
			Resolution = reader.ReadInt32();
			UpdateFrequency = reader.ReadInt32();
			if (IsReadImportance(reader.Version))
			{
				if (IsReadImportanceFirst(reader.Version))
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
			if (IsReadBlendDistance(reader.Version))
			{
				BlendDistance = reader.ReadSingle();
			}
			if (IsReadBakedRenderPassCount(reader.Version))
			{
				BakedRenderPassCount = reader.ReadUInt32();
				UseMipMap = reader.ReadBoolean();
			}
			HDR = reader.ReadBoolean();
			if (IsReadBoxProjection(reader.Version))
			{
				BoxProjection = reader.ReadBoolean();
				RenderDynamicObjects = reader.ReadBoolean();
				UseOcclusionCulling = reader.ReadBoolean();
			}
			if (IsReadImportance(reader.Version))
			{
				if (!IsReadImportanceFirst(reader.Version))
				{
					Importance = reader.ReadInt16();
				}
			}
			reader.AlignStream(AlignType.Align4);
			
			if (IsReadCustomBakedTexture(reader.Version))
			{
				CustomBakedTexture.Read(reader);
			}
			if (IsReadBakedTexture(reader.Version, reader.Flags))
			{
				BakedTextureTexture.Read(reader);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return CustomBakedTexture.FetchDependency(file, isLog, ToLogString, CustomBakedTextureName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
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
			if (IsReadBakedTexture(container.ExportVersion, container.ExportFlags))
			{
				node.Add(BakedTextureName, BakedTextureTexture.ExportYAML(container));
			}
			return node;
		}

		public ReflectionProbeType Type { get; private set; }
		public ReflectionProbeMode Mode { get; private set; }
		public ReflectionProbeRefreshMode RefreshMode { get; private set; }
		public ReflectionProbeTimeSlicingMode TimeSlicingMode { get; private set; }
		public int Resolution { get; private set; }
		public int UpdateFrequency { get; private set; }
		public float NearClip { get; private set; }
		public float FarClip { get; private set; }
		public float ShadowDistance { get; private set; }
		public ReflectionProbeClearFlags ClearFlags { get; private set; }
		public float IntensityMultiplier { get; private set; }
		public float BlendDistance { get; private set; }
		public uint BakedRenderPassCount { get; private set; }
		public bool UseMipMap { get; private set; }
		public bool HDR { get; private set; }
		public bool BoxProjection { get; private set; }
		public bool RenderDynamicObjects { get; private set; }
		public bool UseOcclusionCulling { get; private set; }
		public short Importance { get; private set; }

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
