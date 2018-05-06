using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimationClips
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
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadValueArrayReferencePose(Version version)
		{
			return version.IsGreaterEqual(5, 0);
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

		public void Read(AssetStream stream)
		{
			DeltaPose.Read(stream);
			StartX.Read(stream);
			if (IsReadStopX(stream.Version))
			{
				StopX.Read(stream);
			}
			LeftFootStartX.Read(stream);
			RightFootStartX.Read(stream);

			if (IsReadMotion(stream.Version))
			{
				MotionStartX.Read(stream);
				MotionStopX.Read(stream);
			}

			if(IsVector3(stream.Version))
			{
				AverageSpeed.Read3(stream);
			}
			else
			{
				AverageSpeed.Read(stream);
			}

			Clip.Read(stream);

			StartTime = stream.ReadSingle();
			StopTime = stream.ReadSingle();
			OrientationOffsetY = stream.ReadSingle();
			Level = stream.ReadSingle();
			CycleOffset = stream.ReadSingle();
			AverageAngularSpeed = stream.ReadSingle();

			m_indexArray = stream.ReadInt32Array();
			if (IsReadAdditionalCurveIndexArray(stream.Version))
			{
				m_additionalCurveIndexArray = stream.ReadInt32Array();
			}
			m_valueArrayDelta = stream.ReadArray<ValueDelta>();

			if(IsReadValueArrayReferencePose(stream.Version))
			{
				m_valueArrayReferencePose = stream.ReadSingleArray();
			}

			Mirror = stream.ReadBoolean();
			if (IsReadLoopTime(stream.Version))
			{
				LoopTime = stream.ReadBoolean();
			}
			LoopBlend = stream.ReadBoolean();
			LoopBlendOrientation = stream.ReadBoolean();
			LoopBlendPositionY = stream.ReadBoolean();
			LoopBlendPositionXZ = stream.ReadBoolean();

			if(IsReadStartAtOrigin(stream.Version))
			{
				StartAtOrigin = stream.ReadBoolean();
			}

			KeepOriginalOrientation = stream.ReadBoolean();
			KeepOriginalPositionY = stream.ReadBoolean();
			KeepOriginalPositionXZ = stream.ReadBoolean();
			HeightFromFeet = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
		}
		
		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: value acording to read version (current 2017.3.0f3)
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
