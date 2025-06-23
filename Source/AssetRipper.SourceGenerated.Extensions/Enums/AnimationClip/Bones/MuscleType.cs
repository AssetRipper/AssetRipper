namespace AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.Bones;

public enum MuscleType
{
	SpineFrontBack = 0,
	SpineLeftRight = 1,
	SpineTwistLeftRight = 2,
	ChestFrontBack = 3,
	ChestLeftRight = 4,
	ChestTwistLeftRight = 5,
	UpperchestFrontBack = 6,
	UpperchestLeftRight = 7,
	UpperchestTwisLeftRight = 8,
	NeckNodDownUp = 9,
	NeckTiltLeftRight = 10,
	NeckTurnLeftRight = 11,
	HeadNodDownUp = 12,
	HeadTiltLeftRight = 13,
	HeadTurnLeftRight = 14,
	LeftEyeDownUp = 15,
	LeftEyeInOut = 16,
	RightEyeDownUp = 17,
	RightEyeInOut = 18,
	JawClose = 19,
	JawLeftRight = 20,
	LeftUpperLegFrontBack = 21,
	LeftUpperLegInOut = 22,
	LeftUpperLegTwistInOut = 23,
	LeftLowerLegStretch = 24,
	LeftLowerLegTwistInOut = 25,
	LeftFootUpDown = 26,
	LeftFootTwistInOut = 27,
	LeftToesUpDown = 28,
	RightUpperLegFrontBack = 29,
	RightUpperLegInOut = 30,
	RightUpperLegTwistInOut = 31,
	RightLowerLegStretch = 32,
	RightLowerLegTwistInOut = 33,
	RightFootUpDown = 34,
	RightFootTwistInOut = 35,
	RightToesUpDown = 36,
	LeftShoulderDownUp = 37,
	LeftShoulderFrontBack = 38,
	LeftArmDownUp = 39,
	LeftArmFrontBack = 40,
	LeftArmTwistInOut = 41,
	LeftForearmStretch = 42,
	LeftForearmTwistInOut = 43,
	LeftHandDownUp = 44,
	LeftHandInOut = 45,
	RightShoulderDownUp = 46,
	RightShoulderFrontBack = 47,
	RightArmDownUp = 48,
	RightArmFrontBack = 49,
	RightArmTwistInOut = 50,
	RightForearmStretch = 51,
	RightForearmTwistInOut = 52,
	RightHandDownUp = 53,
	RightHandInOut = 54,

	Last,
}

public static class MuscleTypeExtensions
{
	/// <summary>
	/// 5.6.0 and greater
	/// </summary>
	public static bool IsIncludeUpperChest(UnityVersion version) => BoneTypeExtensions.IsIncludeUpperChest(version);

	public static MuscleType Update(this MuscleType _this, UnityVersion version)
	{
		if (!IsIncludeUpperChest(version))
		{
			if (_this >= MuscleType.UpperchestFrontBack)
			{
				_this += 3;
			}
		}
		return _this;
	}

	public static string ToAttributeString(this MuscleType _this)
	{
		return _this switch
		{
			MuscleType.SpineFrontBack => "Spine Front-Back",
			MuscleType.SpineLeftRight => "Spine Left-Right",
			MuscleType.SpineTwistLeftRight => "Spine Twist Left-Right",
			MuscleType.ChestFrontBack => "Chest Front-Back",
			MuscleType.ChestLeftRight => "Chest Left-Right",
			MuscleType.ChestTwistLeftRight => "Chest Twist Left-Right",
			MuscleType.UpperchestFrontBack => "UpperChest Front-Back",
			MuscleType.UpperchestLeftRight => "UpperChest Left-Right",
			MuscleType.UpperchestTwisLeftRight => "UpperChest Twist Left-Right",
			MuscleType.NeckNodDownUp => "Neck Nod Down-Up",
			MuscleType.NeckTiltLeftRight => "Neck Tilt Left-Right",
			MuscleType.NeckTurnLeftRight => "Neck Turn Left-Right",
			MuscleType.HeadNodDownUp => "Head Nod Down-Up",
			MuscleType.HeadTiltLeftRight => "Head Tilt Left-Right",
			MuscleType.HeadTurnLeftRight => "Head Turn Left-Right",
			MuscleType.LeftEyeDownUp => "Left Eye Down-Up",
			MuscleType.LeftEyeInOut => "Left Eye In-Out",
			MuscleType.RightEyeDownUp => "Right Eye Down-Up",
			MuscleType.RightEyeInOut => "Right Eye In-Out",
			MuscleType.JawClose => "Jaw Close",
			MuscleType.JawLeftRight => "Jaw Left-Right",
			MuscleType.LeftUpperLegFrontBack => "Left Upper Leg Front-Back",
			MuscleType.LeftUpperLegInOut => "Left Upper Leg In-Out",
			MuscleType.LeftUpperLegTwistInOut => "Left Upper Leg Twist In-Out",
			MuscleType.LeftLowerLegStretch => "Left Lower Leg Stretch",
			MuscleType.LeftLowerLegTwistInOut => "Left Lower Leg Twist In-Out",
			MuscleType.LeftFootUpDown => "Left Foot Up-Down",
			MuscleType.LeftFootTwistInOut => "Left Foot Twist In-Out",
			MuscleType.LeftToesUpDown => "Left Toes Up-Down",
			MuscleType.RightUpperLegFrontBack => "Right Upper Leg Front-Back",
			MuscleType.RightUpperLegInOut => "Right Upper Leg In-Out",
			MuscleType.RightUpperLegTwistInOut => "Right Upper Leg Twist In-Out",
			MuscleType.RightLowerLegStretch => "Right Lower Leg Stretch",
			MuscleType.RightLowerLegTwistInOut => "Right Lower Leg Twist In-Out",
			MuscleType.RightFootUpDown => "Right Foot Up-Down",
			MuscleType.RightFootTwistInOut => "Right Foot Twist In-Out",
			MuscleType.RightToesUpDown => "Right Toes Up-Down",
			MuscleType.LeftShoulderDownUp => "Left Shoulder Down-Up",
			MuscleType.LeftShoulderFrontBack => "Left Shoulder Front-Back",
			MuscleType.LeftArmDownUp => "Left Arm Down-Up",
			MuscleType.LeftArmFrontBack => "Left Arm Front-Back",
			MuscleType.LeftArmTwistInOut => "Left Arm Twist In-Out",
			MuscleType.LeftForearmStretch => "Left Forearm Stretch",
			MuscleType.LeftForearmTwistInOut => "Left Forearm Twist In-Out",
			MuscleType.LeftHandDownUp => "Left Hand Down-Up",
			MuscleType.LeftHandInOut => "Left Hand In-Out",
			MuscleType.RightShoulderDownUp => "Right Shoulder Down-Up",
			MuscleType.RightShoulderFrontBack => "Right Shoulder Front-Back",
			MuscleType.RightArmDownUp => "Right Arm Down-Up",
			MuscleType.RightArmFrontBack => "Right Arm Front-Back",
			MuscleType.RightArmTwistInOut => "Right Arm Twist In-Out",
			MuscleType.RightForearmStretch => "Right Forearm Stretch",
			MuscleType.RightForearmTwistInOut => "Right Forearm Twist In-Out",
			MuscleType.RightHandDownUp => "Right Hand Down-Up",
			MuscleType.RightHandInOut => "Right Hand In-Out",
			_ => throw new ArgumentException(_this.ToString()),
		};
	}
}
