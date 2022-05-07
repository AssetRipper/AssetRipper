using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Camera
{
	public sealed class Camera : Behaviour
	{
		public Camera(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// min version is 2nd
			return 2;
		}

		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasProjectionMatrixMode(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2019.1 and greater and Not Release
		/// </summary>
		public static bool HasFOVAxisMode(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(2019);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasSensorSize(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasGateFitMode(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasFocalLength(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasRenderingPath(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasTargetDisplay(UnityVersion version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 5.1.3 and greater
		/// </summary>
		public static bool HasTargetEye(UnityVersion version) => version.IsGreaterEqual(5, 1, 3);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasHDR(UnityVersion version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasAllowMSAA(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasAllowDynamicResolution(UnityVersion version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasForceIntoRT(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasOcclusionCulling(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasStereoConvergence(UnityVersion version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 5.1.0 to 2017.2 exclusive
		/// </summary>
		public static bool HasStereoMirrorMode(UnityVersion version) => version.IsGreaterEqual(5, 1) && version.IsLess(2017, 2);

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		private static bool HasGateFitModeFirst(UnityVersion version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		private static bool IsAlign1(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		private static bool IsAlign2(UnityVersion version) => version.IsGreaterEqual(4, 5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			ClearFlags = reader.ReadUInt32();
			BackGroundColor.Read(reader);
			if (HasProjectionMatrixMode(reader.Version))
			{
				ProjectionMatrixMode = (ProjectionMatrixMode)reader.ReadInt32();
			}
			if (HasGateFitMode(reader.Version))
			{
				if (HasGateFitModeFirst(reader.Version))
				{
					GateFitMode = (GateFitMode)reader.ReadInt32();
				}
			}
			if (IsAlign1(reader.Version))
			{
				reader.AlignStream();
			}
			if (HasSensorSize(reader.Version))
			{
				SensorSize.Read(reader);
				LensShift.Read(reader);
			}
			if (HasGateFitMode(reader.Version))
			{
				if (!HasGateFitModeFirst(reader.Version))
				{
					GateFitMode = (GateFitMode)reader.ReadInt32();
				}
			}
			if (HasFocalLength(reader.Version))
			{
				FocalLength = reader.ReadSingle();
			}

			NormalizedViewPortRect.Read(reader);
			NearClipPlane = reader.ReadSingle();
			FarClipPlane = reader.ReadSingle();
			FieldOfView = reader.ReadSingle();
			Orthographic = reader.ReadBoolean();
			reader.AlignStream();

			OrthographicSize = reader.ReadSingle();
			Depth = reader.ReadSingle();
			CullingMask.Read(reader);
			if (HasRenderingPath(reader.Version))
			{
				RenderingPath = (RenderingPath)reader.ReadInt32();
			}
			TargetTexture.Read(reader);
			if (HasTargetDisplay(reader.Version))
			{
				TargetDisplay = reader.ReadInt32();
			}
			if (HasTargetEye(reader.Version))
			{
				TargetEye = (StereoTargetEyeMask)reader.ReadInt32();
			}
			if (HasHDR(reader.Version))
			{
				HDR = reader.ReadBoolean();
			}
			if (HasAllowMSAA(reader.Version))
			{
				AllowMSAA = reader.ReadBoolean();
			}
			if (HasAllowDynamicResolution(reader.Version))
			{
				AllowDynamicResolution = reader.ReadBoolean();
			}
			if (HasForceIntoRT(reader.Version))
			{
				ForceIntoRT = reader.ReadBoolean();
			}
			if (HasOcclusionCulling(reader.Version))
			{
				OcclusionCulling = reader.ReadBoolean();
			}
			if (IsAlign2(reader.Version))
			{
				reader.AlignStream();
			}
			if (HasStereoConvergence(reader.Version))
			{
				StereoConvergence = reader.ReadSingle();
				StereoSeparation = reader.ReadSingle();
			}
			if (HasStereoMirrorMode(reader.Version))
			{
				StereoMirrorMode = reader.ReadBoolean();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(TargetTexture, TargetTextureName);
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(ClearFlagsName, ClearFlags);
			node.Add(BackGroundColorName, BackGroundColor.ExportYaml(container));

			if (HasProjectionMatrixMode(container.ExportVersion))
			{
				node.Add(ProjectionMatrixModeName, (int)ProjectionMatrixMode);
			}
			if (HasFOVAxisMode(container.ExportVersion, container.ExportFlags))
			{
				node.Add(FOVAxisModeName, (int)FieldOfViewAxis.Vertical);
			}
			if (HasSensorSize(container.ExportVersion))
			{
				node.Add(SensorSizeName, SensorSize.ExportYaml(container));
				node.Add(LensShiftName, LensShift.ExportYaml(container));
			}
			if (HasGateFitMode(container.ExportVersion))
			{
				node.Add(GateFitModeName, (int)GateFitMode);
			}
			if (HasFocalLength(container.ExportVersion))
			{
				node.Add(FocalLengthName, FocalLength);
			}

			node.Add(NormalizedViewPortRectName, NormalizedViewPortRect.ExportYaml(container));
			node.Add(NearClipPlaneName, NearClipPlane);
			node.Add(FarClipPlaneName, FarClipPlane);
			node.Add(FieldOfViewName, FieldOfView);
			node.Add(OrthographicName, Orthographic);
			node.Add(OrthographicSizeName, OrthographicSize);
			node.Add(DepthName, Depth);
			node.Add(CullingMaskName, CullingMask.ExportYaml(container));
			node.Add(RenderingPathName, (int)RenderingPath);
			node.Add(TargetTextureName, TargetTexture.ExportYaml(container));
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

		public uint ClearFlags { get; set; }
		public ProjectionMatrixMode ProjectionMatrixMode { get; set; }
		public GateFitMode GateFitMode { get; set; }
		public float FocalLength { get; set; }
		public float NearClipPlane { get; set; }
		public float FarClipPlane { get; set; }
		public float FieldOfView { get; set; }
		/// <summary>
		/// IsOrthoGraphic previously
		/// </summary>
		public bool Orthographic { get; set; }
		public float OrthographicSize { get; set; }
		public float Depth { get; set; }
		public RenderingPath RenderingPath { get; set; }
		public int TargetDisplay { get; set; }
		public StereoTargetEyeMask TargetEye { get; set; }
		public bool HDR { get; set; }
		public bool AllowMSAA { get; set; }
		public bool AllowDynamicResolution { get; set; }
		public bool ForceIntoRT { get; set; }
		public bool OcclusionCulling { get; set; }
		public float StereoConvergence { get; set; }
		public float StereoSeparation { get; set; }
		public bool StereoMirrorMode { get; set; }

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

		public ColorRGBAf BackGroundColor = new();
		public Vector2f SensorSize = new();
		public Vector2f LensShift = new();
		public Rectf NormalizedViewPortRect = new();
		public BitField CullingMask = new();
		public PPtr<RenderTexture.RenderTexture> TargetTexture = new();
	}
}
