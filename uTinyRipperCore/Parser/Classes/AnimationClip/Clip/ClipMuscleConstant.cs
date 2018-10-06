using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct ClipMuscleConstant : IAssetReadable, IYAMLExportable
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
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
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

			if(IsVector3(reader.Version))
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
			m_valueArrayDelta = reader.ReadArray<ValueDelta>();

			if(IsReadValueArrayReferencePose(reader.Version))
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

			if(IsReadStartAtOrigin(reader.Version))
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
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_AdditiveReferencePoseClip", default(PPtr<AnimationClip>).ExportYAML(container));
			node.Add("m_AdditiveReferencePoseTime", 0);
			node.Add("m_StartTime", StartTime);
			node.Add("m_StopTime", StopTime);
			node.Add("m_OrientationOffsetY", OrientationOffsetY);
			node.Add("m_Level", Level);
			node.Add("m_CycleOffset", CycleOffset);
			node.Add("m_HasAdditiveReferencePose", false);
			node.Add("m_LoopTime", LoopTime);
			node.Add("m_LoopBlend", LoopBlend);
			node.Add("m_LoopBlendOrientation", LoopBlendOrientation);
			node.Add("m_LoopBlendPositionY", LoopBlendPositionY);
			node.Add("m_LoopBlendPositionXZ", LoopBlendPositionXZ);
			node.Add("m_KeepOriginalOrientation", KeepOriginalOrientation);
			node.Add("m_KeepOriginalPositionY", KeepOriginalPositionY);
			node.Add("m_KeepOriginalPositionXZ", KeepOriginalPositionXZ);
			node.Add("m_HeightFromFeet", HeightFromFeet);
			node.Add("m_Mirror", 0);
			return node;
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
