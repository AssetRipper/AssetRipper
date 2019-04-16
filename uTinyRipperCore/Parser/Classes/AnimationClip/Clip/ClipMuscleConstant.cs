using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public class ClipMuscleConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadStopX(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadMotion(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// Less than 4.3.0
		/// </summary>
		public static bool IsReadAdditionalCurveIndexArray(Version version)
		{
			return version.IsLess(4, 3);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadValueArrayReferencePose(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadLoopTime(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadStartAtOrigin(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool IsVector3(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}

		private static int GetSerializedVersion(Version version)
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

		public void Read(AssetReader reader)
		{
			DeltaPose.Read(reader);
			StartX.Read(reader);
			if (IsReadStopX(reader.Version))
			{
				StopX.Read(reader);
			}
			LeftFootStartX.Read(reader);
			RightFootStartX.Read(reader);

			if (IsReadMotion(reader.Version))
			{
				MotionStartX.Read(reader);
				MotionStopX.Read(reader);
			}

			if (IsVector3(reader.Version))
			{
				AverageSpeed.Read3(reader);
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

			m_indexArray = reader.ReadInt32Array();
			if (IsReadAdditionalCurveIndexArray(reader.Version))
			{
				m_additionalCurveIndexArray = reader.ReadInt32Array();
			}
			m_valueArrayDelta = reader.ReadAssetArray<ValueDelta>();

			if (IsReadValueArrayReferencePose(reader.Version))
			{
				m_valueArrayReferencePose = reader.ReadSingleArray();
			}

			Mirror = reader.ReadBoolean();
			if (IsReadLoopTime(reader.Version))
			{
				LoopTime = reader.ReadBoolean();
			}
			LoopBlend = reader.ReadBoolean();
			LoopBlendOrientation = reader.ReadBoolean();
			LoopBlendPositionY = reader.ReadBoolean();
			LoopBlendPositionXZ = reader.ReadBoolean();

			if (IsReadStartAtOrigin(reader.Version))
			{
				StartAtOrigin = reader.ReadBoolean();
			}

			KeepOriginalOrientation = reader.ReadBoolean();
			KeepOriginalPositionY = reader.ReadBoolean();
			KeepOriginalPositionXZ = reader.ReadBoolean();
			HeightFromFeet = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}
		
		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new System.NotImplementedException();
		}
		
		public float StartTime { get; private set; }
		public float StopTime { get; private set; }
		public float OrientationOffsetY { get; private set; }
		public float Level { get; private set; }
		public float CycleOffset { get; private set; }
		public float AverageAngularSpeed { get; private set; }
		public IReadOnlyList<int> IndexArray => m_indexArray;
		public IReadOnlyList<int> AdditionalCurveIndexArray => m_additionalCurveIndexArray;
		public IReadOnlyList<ValueDelta> ValueArrayDelta => m_valueArrayDelta;
		public IReadOnlyList<float> ValueArrayReferencePose => m_valueArrayReferencePose;
		public bool Mirror { get; private set; }
		public bool LoopTime { get; private set; }
		public bool LoopBlend { get; private set; }
		public bool LoopBlendOrientation { get; private set; }
		public bool LoopBlendPositionY { get; private set; }
		public bool LoopBlendPositionXZ { get; private set; }
		public bool StartAtOrigin { get; private set; }
		public bool KeepOriginalOrientation { get; private set; }
		public bool KeepOriginalPositionY { get; private set; }
		public bool KeepOriginalPositionXZ { get; private set; }
		public bool HeightFromFeet { get; private set; }

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
		
		private int[] m_indexArray;
		private int[] m_additionalCurveIndexArray;
		private ValueDelta[] m_valueArrayDelta;
		private float[] m_valueArrayReferencePose;
	}
}
