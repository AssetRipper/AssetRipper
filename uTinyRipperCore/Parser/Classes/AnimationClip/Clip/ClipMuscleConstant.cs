using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public class ClipMuscleConstant : IAssetReadable, IYAMLExportable
	{
		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5, 6))
			{
				return 3;
			}
			if (version.IsGreaterEqual(4, 3))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasStopX(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasMotion(Version version) => version.IsLess(5);
		/// <summary>
		/// Less than 4.3.0
		/// </summary>
		public static bool HasAdditionalCurveIndexArray(Version version) => version.IsLess(4, 3);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasValueArrayReferencePose(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasLoopTime(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasStartAtOrigin(Version version) => version.IsGreaterEqual(5, 5);
		
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool IsVector3(Version version) => version.IsGreaterEqual(5, 4);

		public void Read(AssetReader reader)
		{
			DeltaPose.Read(reader);
			StartX.Read(reader);
			if (HasStopX(reader.Version))
			{
				StopX.Read(reader);
			}
			LeftFootStartX.Read(reader);
			RightFootStartX.Read(reader);

			if (HasMotion(reader.Version))
			{
				MotionStartX.Read(reader);
				MotionStopX.Read(reader);
			}

			if (IsVector3(reader.Version))
			{
				AverageSpeed = reader.ReadAsset<Vector3f>();
			}
			else
			{
				AverageSpeed.Read(reader);
			}

			Clip.Read(reader);

			StartTime = reader.ReadSingle();
			StopTime = reader.ReadSingle();
			OrientationOffsetY = reader.ReadSingle();
			Level = reader.ReadSingle();
			CycleOffset = reader.ReadSingle();
			AverageAngularSpeed = reader.ReadSingle();

			IndexArray = reader.ReadInt32Array();
			if (HasAdditionalCurveIndexArray(reader.Version))
			{
				AdditionalCurveIndexArray = reader.ReadInt32Array();
			}
			ValueArrayDelta = reader.ReadAssetArray<ValueDelta>();

			if (HasValueArrayReferencePose(reader.Version))
			{
				ValueArrayReferencePose = reader.ReadSingleArray();
			}

			Mirror = reader.ReadBoolean();
			if (HasLoopTime(reader.Version))
			{
				LoopTime = reader.ReadBoolean();
			}
			LoopBlend = reader.ReadBoolean();
			LoopBlendOrientation = reader.ReadBoolean();
			LoopBlendPositionY = reader.ReadBoolean();
			LoopBlendPositionXZ = reader.ReadBoolean();

			if (HasStartAtOrigin(reader.Version))
			{
				StartAtOrigin = reader.ReadBoolean();
			}

			KeepOriginalOrientation = reader.ReadBoolean();
			KeepOriginalPositionY = reader.ReadBoolean();
			KeepOriginalPositionXZ = reader.ReadBoolean();
			HeightFromFeet = reader.ReadBoolean();
			reader.AlignStream();
		}
		
		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new System.NotImplementedException();
		}
		
		public float StartTime { get; set; }
		public float StopTime { get; set; }
		public float OrientationOffsetY { get; set; }
		public float Level { get; set; }
		public float CycleOffset { get; set; }
		public float AverageAngularSpeed { get; set; }
		public int[] IndexArray { get; set; }
		public int[] AdditionalCurveIndexArray { get; set; }
		public ValueDelta[] ValueArrayDelta { get; set; }
		public float[] ValueArrayReferencePose { get; set; }
		public bool Mirror { get; set; }
		public bool LoopTime { get; set; }
		public bool LoopBlend { get; set; }
		public bool LoopBlendOrientation { get; set; }
		public bool LoopBlendPositionY { get; set; }
		public bool LoopBlendPositionXZ { get; set; }
		public bool StartAtOrigin { get; set; }
		public bool KeepOriginalOrientation { get; set; }
		public bool KeepOriginalPositionY { get; set; }
		public bool KeepOriginalPositionXZ { get; set; }
		public bool HeightFromFeet { get; set; }

		public const string AdditiveReferencePoseClipName = "m_AdditiveReferencePoseClip";
		public const string AdditiveReferencePoseTimeName = "m_AdditiveReferencePoseTime";
		public const string StartTimeName = "m_StartTime";
		public const string StopTimeName = "m_StopTime";
		public const string OrientationOffsetYName = "m_OrientationOffsetY";
		public const string LevelName = "m_Level";
		public const string CycleOffsetName = "m_CycleOffset";
		public const string HasAdditiveReferencePoseName = "m_HasAdditiveReferencePose";
		public const string LoopTimeName = "m_LoopTime";
		public const string LoopBlendName = "m_LoopBlend";
		public const string LoopBlendOrientationName = "m_LoopBlendOrientation";
		public const string LoopBlendPositionYName = "m_LoopBlendPositionY";
		public const string LoopBlendPositionXZName = "m_LoopBlendPositionXZ";
		public const string KeepOriginalOrientationName = "m_KeepOriginalOrientation";
		public const string KeepOriginalPositionYName = "m_KeepOriginalPositionY";
		public const string KeepOriginalPositionXZName = "m_KeepOriginalPositionXZ";
		public const string HeightFromFeetName = "m_HeightFromFeet";
		public const string MirrorName = "m_Mirror";

		public HumanPose DeltaPose;
		public XForm StartX;
		public XForm StopX;
		public XForm LeftFootStartX;
		public XForm RightFootStartX;
		public XForm MotionStartX;
		public XForm MotionStopX;
		public Vector4f AverageSpeed;
		public Clip Clip;
	}
}
