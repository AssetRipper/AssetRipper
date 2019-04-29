using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Cameras;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class Camera : Behaviour
	{
		public Camera(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadProjectionMatrixMode(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 2019.1 and greater and Not Release
		/// </summary>
		public static bool IsReadFOVAxisMode(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadSensorSize(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadGateFitMode(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadFocalLength(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadRenderingPath(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool IsReadTargetDisplay(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 5.1.3 and greater
		/// </summary>
		public static bool IsReadTargetEye(Version version)
		{
			return version.IsGreaterEqual(5, 1, 3);
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadHDR(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadAllowMSAA(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadAllowDynamicResolution(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadForceIntoRT(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadOcclusionCulling(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool IsReadStereoConvergence(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 5.1.0 to 2017.2 exclusive
		/// </summary>
		public static bool IsReadStereoMirrorMode(Version version)
		{
			return version.IsGreaterEqual(5, 1) && version.IsLess(2017, 2);
		}
		
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		private static bool IsReadGateFitModeFirst(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		private static bool IsAlign1(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		private static bool IsAlign2(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		
		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}
			// min version is 2nd
			return 2;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			ClearFlags = reader.ReadUInt32();
			BackGroundColor.Read(reader);
			if (IsReadProjectionMatrixMode(reader.Version))
			{
				ProjectionMatrixMode = (ProjectionMatrixMode)reader.ReadInt32();
			}
			if (IsReadGateFitMode(reader.Version))
			{
				if (IsReadGateFitModeFirst(reader.Version))
				{
					GateFitMode = (GateFitMode)reader.ReadInt32();
				}
			}
#if UNIVERSAL
			if (IsReadFOVAxisMode(reader.Version, reader.Flags))
			{
				FOVAxisMode = (FieldOfViewAxis)reader.ReadInt32();
			}
#endif
			if (IsAlign1(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadSensorSize(reader.Version))
			{
				SensorSize.Read(reader);
				LensShift.Read(reader);
			}
			if (IsReadGateFitMode(reader.Version))
			{
				if (!IsReadGateFitModeFirst(reader.Version))
				{
					GateFitMode = (GateFitMode)reader.ReadInt32();
				}
			}
			if (IsReadFocalLength(reader.Version))
			{
				FocalLength = reader.ReadSingle();
			}

			NormalizedViewPortRect.Read(reader);
			NearClipPlane = reader.ReadSingle();
			FarClipPlane = reader.ReadSingle();
			FieldOfView = reader.ReadSingle();
			Orthographic = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
			
			OrthographicSize = reader.ReadSingle();
			Depth = reader.ReadSingle();
			CullingMask.Read(reader);
			if (IsReadRenderingPath(reader.Version))
			{
				RenderingPath = (RenderingPath)reader.ReadInt32();
			}
			TargetTexture.Read(reader);
			if (IsReadTargetDisplay(reader.Version))
			{
				TargetDisplay = reader.ReadInt32();
			}
			if (IsReadTargetEye(reader.Version))
			{
				TargetEye = (StereoTargetEyeMask)reader.ReadInt32();
			}
			if (IsReadHDR(reader.Version))
			{
				HDR = reader.ReadBoolean();
			}
			if (IsReadAllowMSAA(reader.Version))
			{
				AllowMSAA = reader.ReadBoolean();
			}
			if (IsReadAllowDynamicResolution(reader.Version))
			{
				AllowDynamicResolution = reader.ReadBoolean();
			}
			if (IsReadForceIntoRT(reader.Version))
			{
				ForceIntoRT = reader.ReadBoolean();
			}
			if (IsReadOcclusionCulling(reader.Version))
			{
				OcclusionCulling = reader.ReadBoolean();
			}
			if (IsAlign2(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadStereoConvergence(reader.Version))
			{
				StereoConvergence = reader.ReadSingle();
				StereoSeparation = reader.ReadSingle();
			}
			if (IsReadStereoMirrorMode(reader.Version))
			{
				StereoMirrorMode = reader.ReadBoolean();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			yield return TargetTexture.FetchDependency(file, isLog, ToLogString, "m_TargetTexture");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add(ClearFlagsName, ClearFlags);
			node.Add(BackGroundColorName, BackGroundColor.ExportYAML(container));

			if (IsReadProjectionMatrixMode(container.ExportVersion))
			{
				node.Add(ProjectionMatrixModeName, (int)ProjectionMatrixMode);
			}
			if (IsReadFOVAxisMode(container.ExportVersion, container.ExportFlags))
			{
				node.Add(FOVAxisModeName, (int)GetFOVAxisMode(container.Version, container.Flags));
			}
			if (IsReadSensorSize(container.ExportVersion))
			{
				node.Add(SensorSizeName, SensorSize.ExportYAML(container));
				node.Add(LensShiftName, LensShift.ExportYAML(container));
			}
			if (IsReadGateFitMode(container.ExportVersion))
			{
				node.Add(GateFitModeName, (int)GateFitMode);
			}
			if (IsReadFocalLength(container.ExportVersion))
			{
				node.Add(FocalLengthName, FocalLength);
			}

			node.Add(NormalizedViewPortRectName, NormalizedViewPortRect.ExportYAML(container));
			node.Add(NearClipPlaneName, NearClipPlane);
			node.Add(FarClipPlaneName, FarClipPlane);
			node.Add(FieldOfViewName, FieldOfView);
			node.Add(OrthographicName, Orthographic);
			node.Add(OrthographicSizeName, OrthographicSize);
			node.Add(DepthName, Depth);
			node.Add(CullingMaskName, CullingMask.ExportYAML(container));
			node.Add(RenderingPathName, (int)RenderingPath);
			node.Add(TargetTextureName, TargetTexture.ExportYAML(container));
			node.Add(TargetDisplayName, TargetDisplay);
			node.Add(TargetEyeName, (int)TargetEye);
			node.Add(HDRName, HDR);
			node.Add(AllowMSAAName, AllowMSAA);
			node.Add(AllowDynamicResolutionName, AllowDynamicResolution);
			node.Add(ForceIntoRTName, ForceIntoRT);
			node.Add(OcclusionCullingName, OcclusionCulling);
			node.Add(StereoConvergenceName, StereoConvergence);
			node.Add(StereoSeparationName, StereoSeparation);
			return node;
		}

		private FieldOfViewAxis GetFOVAxisMode(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadFOVAxisMode(version, flags))
			{
				return FOVAxisMode;
			}
#endif
			return FieldOfViewAxis.Vertical;
		}

		public uint ClearFlags { get; private set; }
		public ProjectionMatrixMode ProjectionMatrixMode { get; private set; }
		public GateFitMode GateFitMode { get; private set; }
#if UNIVERSAL
		public FieldOfViewAxis FOVAxisMode { get; private set; }
#endif
		public float FocalLength { get; private set; }
		public float NearClipPlane { get; private set; }
		public float FarClipPlane { get; private set; }
		public float FieldOfView { get; private set; }
		/// <summary>
		/// IsOrthoGraphic previously
		/// </summary>
		public bool Orthographic { get; private set; }
		public float OrthographicSize { get; private set; }
		public float Depth { get; private set; }
		public RenderingPath RenderingPath { get; private set; }
		public int TargetDisplay { get; private set; }
		public StereoTargetEyeMask TargetEye { get; private set; }
		public bool HDR { get; private set; }
		public bool AllowMSAA { get; private set; }
		public bool AllowDynamicResolution { get; private set; }
		public bool ForceIntoRT { get; private set; }
		public bool OcclusionCulling { get; private set; }
		public float StereoConvergence { get; private set; }
		public float StereoSeparation { get; private set; }
		public bool StereoMirrorMode { get; private set; }

		public const string ClearFlagsName = "m_ClearFlags";
		public const string BackGroundColorName = "m_BackGroundColor";
		public const string ProjectionMatrixModeName = "m_projectionMatrixMode";
		public const string FOVAxisModeName = "m_FOVAxisMode";
		public const string SensorSizeName = "m_SensorSize";
		public const string LensShiftName = "m_LensShift";
		public const string GateFitModeName = "m_GateFitMode";
		public const string FocalLengthName = "m_FocalLength";
		public const string NormalizedViewPortRectName = "m_NormalizedViewPortRect";
		public const string NearClipPlaneName = "near clip plane";
		public const string FarClipPlaneName = "far clip plane";
		public const string FieldOfViewName = "field of view";
		public const string OrthographicName = "orthographic";
		public const string OrthographicSizeName = "orthographic size";
		public const string DepthName = "m_Depth";
		public const string CullingMaskName = "m_CullingMask";
		public const string RenderingPathName = "m_RenderingPath";
		public const string TargetTextureName = "m_TargetTexture";
		public const string TargetDisplayName = "m_TargetDisplay";
		public const string TargetEyeName = "m_TargetEye";
		public const string HDRName = "m_HDR";
		public const string AllowMSAAName = "m_AllowMSAA";
		public const string AllowDynamicResolutionName = "m_AllowDynamicResolution";
		public const string ForceIntoRTName = "m_ForceIntoRT";
		public const string OcclusionCullingName = "m_OcclusionCulling";
		public const string StereoConvergenceName = "m_StereoConvergence";
		public const string StereoSeparationName = "m_StereoSeparation";

		public ColorRGBAf BackGroundColor;
		public Vector2f SensorSize;
		public Vector2f LensShift;
		public Rectf NormalizedViewPortRect;
		public BitField CullingMask;
		public PPtr<RenderTexture> TargetTexture;
	}
}
