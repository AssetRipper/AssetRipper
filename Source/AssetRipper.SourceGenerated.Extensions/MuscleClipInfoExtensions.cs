using AssetRipper.SourceGenerated.Subclasses.MuscleClipConstant;
using AssetRipper.SourceGenerated.Subclasses.MuscleClipInfo;

namespace AssetRipper.SourceGenerated.Extensions;

public static class MuscleClipInfoExtensions
{
	public static void Initialize(this IMuscleClipInfo info)
	{
		info.StopTime = 1.0f;
		info.KeepOriginalPositionY = true;
	}

	public static void Initialize(this IMuscleClipInfo info, IMuscleClipConstant muscleConst)
	{
		info.AdditiveReferencePoseTime = 0.0f;
		info.StartTime = muscleConst.StartTime;
		info.StopTime = muscleConst.StopTime;
		info.OrientationOffsetY = muscleConst.OrientationOffsetY;
		info.Level = muscleConst.Level;
		info.CycleOffset = muscleConst.CycleOffset;
		info.HasAdditiveReferencePose = false;
		info.LoopTime = muscleConst.LoopTime;
		info.LoopBlend = muscleConst.LoopBlend;
		info.LoopBlendOrientation = muscleConst.LoopBlendOrientation;
		info.LoopBlendPositionY = muscleConst.LoopBlendPositionY;
		info.LoopBlendPositionXZ = muscleConst.LoopBlendPositionXZ;
		info.KeepOriginalOrientation = muscleConst.KeepOriginalOrientation;
		info.KeepOriginalPositionY = muscleConst.KeepOriginalPositionY;
		info.KeepOriginalPositionXZ = muscleConst.KeepOriginalPositionXZ;
		info.HeightFromFeet = muscleConst.HeightFromFeet;
		info.Mirror = false;
	}
}
