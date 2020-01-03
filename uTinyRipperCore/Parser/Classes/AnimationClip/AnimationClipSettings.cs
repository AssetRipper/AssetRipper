using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	/// <summary>
	/// MuscleClipInfo previously
	/// </summary>
	public class AnimationClipSettings : IAssetReadable, IYAMLExportable, IDependent
	{
		public AnimationClipSettings()
		{
		}

		public AnimationClipSettings(bool _)
		{
			StopTime = 1.0f;
			KeepOriginalPositionY = true;
		}

		public AnimationClipSettings(ClipMuscleConstant muscleConst)
		{
			AdditiveReferencePoseClip = default;
			AdditiveReferencePoseTime = 0.0f;
			StartTime = muscleConst.StartTime;
			StopTime = muscleConst.StopTime;
			OrientationOffsetY = muscleConst.OrientationOffsetY;
			Level = muscleConst.Level;
			CycleOffset = muscleConst.CycleOffset;
			HasAdditiveReferencePose = false;
			LoopTime = muscleConst.LoopTime;
			LoopBlend = muscleConst.LoopBlend;
			LoopBlendOrientation = muscleConst.LoopBlendOrientation;
			LoopBlendPositionY = muscleConst.LoopBlendPositionY;
			LoopBlendPositionXZ = muscleConst.LoopBlendPositionXZ;
			KeepOriginalOrientation = muscleConst.KeepOriginalOrientation;
			KeepOriginalPositionY = muscleConst.KeepOriginalPositionY;
			KeepOriginalPositionXZ = muscleConst.KeepOriginalPositionXZ;
			HeightFromFeet = muscleConst.HeightFromFeet;
			Mirror = false;
		}

		public static int ToSerializedVersion(Version version)
		{
			// LoopBlend has been splitted to LoopBlend and LoopTime
			if (version.IsGreaterEqual(4, 3))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasAdditiveReferencePoseClip(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasHasAdditiveReferencePose(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasLoopTime(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// Less than 4.1.0
		/// </summary>
		public static bool HasKeepAdditionalBonesAnimation(Version version) => version.IsLess(4, 1);

		public void Read(AssetReader reader)
		{
			if (HasAdditiveReferencePoseClip(reader.Version))
			{
				AdditiveReferencePoseClip.Read(reader);
				AdditiveReferencePoseTime = reader.ReadSingle();
			}
			StartTime = reader.ReadSingle();
			StopTime = reader.ReadSingle();
			OrientationOffsetY = reader.ReadSingle();
			Level = reader.ReadSingle();
			CycleOffset = reader.ReadSingle();
			if (HasHasAdditiveReferencePose(reader.Version))
			{
				HasAdditiveReferencePose = reader.ReadBoolean();
			}
			if (HasLoopTime(reader.Version))
			{
				LoopTime = reader.ReadBoolean();
			}
			LoopBlend = reader.ReadBoolean();
			LoopBlendOrientation = reader.ReadBoolean();
			LoopBlendPositionY = reader.ReadBoolean();
			LoopBlendPositionXZ = reader.ReadBoolean();
			KeepOriginalOrientation = reader.ReadBoolean();
			KeepOriginalPositionY = reader.ReadBoolean();
			KeepOriginalPositionXZ = reader.ReadBoolean();
			HeightFromFeet = reader.ReadBoolean();
			Mirror = reader.ReadBoolean();
			if (HasKeepAdditionalBonesAnimation(reader.Version))
			{
				KeepAdditionalBonesAnimation = reader.ReadBoolean();
			}
			reader.AlignStream();
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			if (HasAdditiveReferencePoseClip(context.Version))
			{
				yield return context.FetchDependency(AdditiveReferencePoseClip, AdditiveReferencePoseClipName);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(AdditiveReferencePoseClipName, AdditiveReferencePoseClip.ExportYAML(container));
			node.Add(AdditiveReferencePoseTimeName, AdditiveReferencePoseTime);
			node.Add(StartTimeName, StartTime);
			node.Add(StopTimeName, StopTime);
			node.Add(OrientationOffsetYName, OrientationOffsetY);
			node.Add(LevelName, Level);
			node.Add(CycleOffsetName, CycleOffset);
			node.Add(HasAdditiveReferencePoseName, HasAdditiveReferencePose);
			node.Add(LoopTimeName, GetLoopTime(container.Version));
			node.Add(LoopBlendName, LoopBlend);
			node.Add(LoopBlendOrientationName, LoopBlendOrientation);
			node.Add(LoopBlendPositionYName, LoopBlendPositionY);
			node.Add(LoopBlendPositionXZName, LoopBlendPositionXZ);
			node.Add(KeepOriginalOrientationName, KeepOriginalOrientation);
			node.Add(KeepOriginalPositionYName, KeepOriginalPositionY);
			node.Add(KeepOriginalPositionXZName, KeepOriginalPositionXZ);
			node.Add(HeightFromFeetName, HeightFromFeet);
			node.Add(MirrorName, Mirror);
			return node;
		}

		private bool GetLoopTime(Version version)
		{
			return HasLoopTime(version) ? LoopTime : LoopBlend;
		}

		public float AdditiveReferencePoseTime { get; set; }
		public float StartTime { get; set; }
		public float StopTime { get; set; }
		public float OrientationOffsetY { get; set; }
		public float Level { get; set; }
		public float CycleOffset { get; set; }
		public bool HasAdditiveReferencePose { get; set; }
		public bool LoopTime { get; set; }
		public bool LoopBlend { get; set; }
		public bool LoopBlendOrientation { get; set; }
		public bool LoopBlendPositionY { get; set; }
		public bool LoopBlendPositionXZ { get; set; }
		public bool KeepOriginalOrientation { get; set; }
		public bool KeepOriginalPositionY { get; set; }
		public bool KeepOriginalPositionXZ { get; set; }
		public bool HeightFromFeet { get; set; }
		public bool Mirror { get; set; }
		public bool KeepAdditionalBonesAnimation { get; set; }

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
		public const string KeepAdditionalBonesAnimationName = "m_KeepAdditionalBonesAnimation";

		public PPtr<AnimationClip> AdditiveReferencePoseClip;
	}
}
