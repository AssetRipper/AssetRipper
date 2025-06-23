using AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip;
using AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.Bones;

namespace AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip;

public enum HumanoidMuscleType
{
	Motion = 0,
	Root = Motion + 7,
	Limbs = Root + 7,
	Muscles = Limbs + LimbType.Last * 7,
	Fingers = Muscles + MuscleType.Last,
	TDoFBones = Fingers + ArmType.Last * FingerType.Last * FingerDoFType.Last,

	Last = TDoFBones + TDoFBoneType.Last * 3,
}

public static class HumanoidMuscleTypeExtensions
{
	public static HumanoidMuscleType Update(this HumanoidMuscleType _this, UnityVersion version)
	{
		if (_this < HumanoidMuscleType.Muscles)
		{
			return _this;
		}

		MuscleType muscle = (MuscleType)(_this - HumanoidMuscleType.Muscles);
		MuscleType fixedMuscle = muscle.Update(version);
		_this = HumanoidMuscleType.Muscles + (int)fixedMuscle;
		if (_this < HumanoidMuscleType.TDoFBones)
		{
			return _this;
		}

		TDoFBoneType tdof = (TDoFBoneType)(_this - HumanoidMuscleType.TDoFBones);
		TDoFBoneType fixedTdof = tdof.Update(version);
		_this = HumanoidMuscleType.TDoFBones + (int)fixedTdof;
		return _this;
	}

	public static string ToAttributeString(this HumanoidMuscleType _this)
	{
		if (_this < HumanoidMuscleType.Root)
		{
			int delta = _this - HumanoidMuscleType.Motion;
			return nameof(HumanoidMuscleType.Motion) + GetTransformPostfix(delta % 7);
		}
		if (_this < HumanoidMuscleType.Limbs)
		{
			int delta = _this - HumanoidMuscleType.Root;
			return nameof(HumanoidMuscleType.Root) + GetTransformPostfix(delta % 7);
		}
		if (_this < HumanoidMuscleType.Muscles)
		{
			int delta = _this - HumanoidMuscleType.Limbs;
			LimbType limb = (LimbType)(delta / 7);
			return limb.ToBoneType().ToAttributeString() + GetTransformPostfix(delta % 7);
		}
		if (_this < HumanoidMuscleType.Fingers)
		{
			int delta = _this - HumanoidMuscleType.Muscles;
			MuscleType muscle = (MuscleType)delta;
			return muscle.ToAttributeString();
		}
		if (_this < HumanoidMuscleType.TDoFBones)
		{
			const int armSize = (int)FingerType.Last * (int)FingerDoFType.Last;
			const int dofSize = (int)FingerDoFType.Last;
			int delta = _this - HumanoidMuscleType.Fingers;
			ArmType arm = (ArmType)(delta / armSize);
			delta %= armSize;
			FingerType finger = (FingerType)(delta / dofSize);
			delta %= dofSize;
			FingerDoFType dof = (FingerDoFType)delta;
			return $"{arm.ToBoneType().ToAttributeString()}.{finger.ToAttributeString()}.{dof.ToAttributeString()}";
		}
		if (_this < HumanoidMuscleType.Last)
		{
			int delta = _this - HumanoidMuscleType.TDoFBones;
			TDoFBoneType tdof = (TDoFBoneType)(delta / 3);
			return $"{tdof.ToBoneType().ToAttributeString()}{GetTDoFTransformPostfix(delta % 3)}";
		}
		throw new ArgumentException(_this.ToString());
	}

	private static string GetTransformPostfix(int index)
	{
		return index switch
		{
			0 => "T.x",
			1 => "T.y",
			2 => "T.z",
			3 => "Q.x",
			4 => "Q.y",
			5 => "Q.z",
			6 => "Q.w",
			_ => throw new ArgumentException(index.ToString()),
		};
	}

	private static string GetTDoFTransformPostfix(int index)
	{
		return index switch
		{
			0 => "TDOF.x",
			1 => "TDOF.y",
			2 => "TDOF.z",
			_ => throw new ArgumentException(index.ToString()),
		};
	}
}
