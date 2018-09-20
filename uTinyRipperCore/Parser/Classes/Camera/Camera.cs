using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Cameras;
using uTinyRipper.Exporter.YAML;
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
		/// 4.5.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
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
			if(IsReadProjectionMatrixMode(reader.Version))
			{
				ProjectionMatrixMode = (ProjectionMatrixMode)reader.ReadInt32();
				SensorSize.Read(reader);
				LensShift.Read(reader);
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
			if (IsAlign(reader.Version))
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
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			
			yield return TargetTexture.FetchDependency(file, isLog, ToLogString, "m_TargetTexture");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_ClearFlags", ClearFlags);
			node.Add("m_BackGroundColor", BackGroundColor.ExportYAML(container));
			node.Add("m_NormalizedViewPortRect", NormalizedViewPortRect.ExportYAML(container));
			node.Add("near clip plane", NearClipPlane);
			node.Add("far clip plane", FarClipPlane);
			node.Add("field of view", FieldOfView);
			node.Add("orthographic", Orthographic);
			node.Add("orthographic size", OrthographicSize);
			node.Add("m_Depth", Depth);
			node.Add("m_CullingMask", CullingMask.ExportYAML(container));
			node.Add("m_RenderingPath", (int)RenderingPath);
			node.Add("m_TargetTexture", TargetTexture.ExportYAML(container));
			node.Add("m_TargetDisplay", TargetDisplay);
			node.Add("m_TargetEye", (int)TargetEye);
			node.Add("m_HDR", HDR);
			node.Add("m_AllowMSAA", AllowMSAA);
			node.Add("m_AllowDynamicResolution", AllowDynamicResolution);
			node.Add("m_ForceIntoRT", ForceIntoRT);
			node.Add("m_OcclusionCulling", OcclusionCulling);
			node.Add("m_StereoConvergence", StereoConvergence);
			node.Add("m_StereoSeparation", StereoSeparation);
			return node;
		}

		public uint ClearFlags { get; private set; }
		public ProjectionMatrixMode ProjectionMatrixMode { get; private set; }
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

		public ColorRGBAf BackGroundColor;
		public Vector2f SensorSize;
		public Vector2f LensShift;
		public Rectf NormalizedViewPortRect;
		public BitField CullingMask;
		public PPtr<RenderTexture> TargetTexture;
	}
}
