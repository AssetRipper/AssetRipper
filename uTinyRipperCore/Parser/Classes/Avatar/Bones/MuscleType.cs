using System;

namespace uTinyRipper.Classes.Avatars
{
	public enum MuscleType
	{
		SpineFrontBack				= 0,
		SpineLeftRight				= 1,
		SpineTwistLeftRight			= 2,
		ChestFrontBack				= 3,
		ChestLeftRight				= 4,
		ChestTwistLeftRight			= 5,
		UpperchestFrontBack			= 6,
		UpperchestLeftRight			= 7,
		UpperchestTwisLeftRight		= 8,
		NeckNodDownUp				= 9,
		NeckTiltLeftRight			= 10,
		NeckTurnLeftRight			= 11,
		HeadNodDownUp				= 12,
		HeadTiltLeftRight			= 13,
		HeadTurnLeftRight			= 14,
		LeftEyeDownUp				= 15,
		LeftEyeInOut				= 16,
		RightEyeDownUp				= 17,
		RightEyeInOut				= 18,
		JawClose					= 19,
		JawLeftRight				= 20,
		LeftUpperLegFrontBack		= 21,
		LeftUpperLegInOut			= 22,
		LeftUpperLegTwistInOut		= 23,
		LeftLowerLegStretch			= 24,
		LeftLowerLegTwistInOut		= 25,
		LeftFootUpDown				= 26,
		LeftFootTwistInOut			= 27,
		LeftToesUpDown				= 28,
		RightUpperLegFrontBack		= 29,
		RightUpperLegInOut			= 30,
		RightUpperLegTwistInOut		= 31,
		RightLowerLegStretch		= 32,
		RightLowerLegTwistInOut		= 33,
		RightFootUpDown				= 34,
		RightFootTwistInOut			= 35,
		RightToesUpDown				= 36,
		LeftShoulderDownUp			= 37,
		LeftShoulderFrontBack		= 38,
		LeftArmDownUp				= 39,
		LeftArmFrontBack			= 40,
		LeftArmTwistInOut			= 41,
		LeftForearmStretch			= 42,
		LeftForearmTwistInOut		= 43,
		LeftHandDownUp				= 44,
		LeftHandInOut				= 45,
		RightShoulderDownUp			= 46,
		RightShoulderFrontBack		= 47,
		RightArmDownUp				= 48,
		RightArmFrontBack			= 49,
		RightArmTwistInOut			= 50,
		RightForearmStretch			= 51,
		RightForearmTwistInOut		= 52,
		RightHandDownUp				= 53,
		RightHandInOut				= 54,

		Last,
	}

	public static class MuscleTypeExtensions
	{
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsIncludeUpperChest(Version version)
		{
			return BoneTypeExtensions.IsIncludeUpperChest(version);
		}

		public static MuscleType Update(this MuscleType _this, Version version)
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
			switch(_this)
			{
				case MuscleType.SpineFrontBack:
					return "Spine Front-Back";
				case MuscleType.SpineLeftRight:
					return "Spine Left-Right";
				case MuscleType.SpineTwistLeftRight:
					return "Spine Twist Left-Right";
				case MuscleType.ChestFrontBack:
					return "Chest Front-Back";
				case MuscleType.ChestLeftRight:
					return "Chest Left-Right";
				case MuscleType.ChestTwistLeftRight:
					return "Chest Twist Left-Right";
				case MuscleType.UpperchestFrontBack:
					return "UpperChest Front-Back";
				case MuscleType.UpperchestLeftRight:
					return "UpperChest Left-Right";
				case MuscleType.UpperchestTwisLeftRight:
					return "UpperChest Twist Left-Right";
				case MuscleType.NeckNodDownUp:
					return "Neck Nod Down-Up";
				case MuscleType.NeckTiltLeftRight:
					return "Neck Tilt Left-Right";
				case MuscleType.NeckTurnLeftRight:
					return "Neck Turn Left-Right";
				case MuscleType.HeadNodDownUp:
					return "Head Nod Down-Up";
				case MuscleType.HeadTiltLeftRight:
					return "Head Tilt Left-Right";
				case MuscleType.HeadTurnLeftRight:
					return "Head Turn Left-Right";
				case MuscleType.LeftEyeDownUp:
					return "Left Eye Down-Up";
				case MuscleType.LeftEyeInOut:
					return "Left Eye In-Out";
				case MuscleType.RightEyeDownUp:
					return "Right Eye Down-Up";
				case MuscleType.RightEyeInOut:
					return "Right Eye In-Out";
				case MuscleType.JawClose:
					return "Jaw Close";
				case MuscleType.JawLeftRight:
					return "Jaw Left-Right";
				case MuscleType.LeftUpperLegFrontBack:
					return "Left Upper Leg Front-Back";
				case MuscleType.LeftUpperLegInOut:
					return "Left Upper Leg In-Out";
				case MuscleType.LeftUpperLegTwistInOut:
					return "Left Upper Leg Twist In-Out";
				case MuscleType.LeftLowerLegStretch:
					return "Left Lower Leg Stretch";
				case MuscleType.LeftLowerLegTwistInOut:
					return "Left Lower Leg Twist In-Out";
				case MuscleType.LeftFootUpDown:
					return "Left Foot Up-Down";
				case MuscleType.LeftFootTwistInOut:
					return "Left Foot Twist In-Out";
				case MuscleType.LeftToesUpDown:
					return "Left Toes Up-Down";
				case MuscleType.RightUpperLegFrontBack:
					return "Right Upper Leg Front-Back";
				case MuscleType.RightUpperLegInOut:
					return "Right Upper Leg In-Out";
				case MuscleType.RightUpperLegTwistInOut:
					return "Right Upper Leg Twist In-Out";
				case MuscleType.RightLowerLegStretch:
					return "Right Lower Leg Stretch";
				case MuscleType.RightLowerLegTwistInOut:
					return "Right Lower Leg Twist In-Out";
				case MuscleType.RightFootUpDown:
					return "Right Foot Up-Down";
				case MuscleType.RightFootTwistInOut:
					return "Right Foot Twist In-Out";
				case MuscleType.RightToesUpDown:
					return "Right Toes Up-Down";
				case MuscleType.LeftShoulderDownUp:
					return "Left Shoulder Down-Up";
				case MuscleType.LeftShoulderFrontBack:
					return "Left Shoulder Front-Back";
				case MuscleType.LeftArmDownUp:
					return "Left Arm Down-Up";
				case MuscleType.LeftArmFrontBack:
					return "Left Arm Front-Back";
				case MuscleType.LeftArmTwistInOut:
					return "Left Arm Twist In-Out";
				case MuscleType.LeftForearmStretch:
					return "Left Forearm Stretch";
				case MuscleType.LeftForearmTwistInOut:
					return "Left Forearm Twist In-Out";
				case MuscleType.LeftHandDownUp:
					return "Left Hand Down-Up";
				case MuscleType.LeftHandInOut:
					return "Left Hand In-Out";
				case MuscleType.RightShoulderDownUp:
					return "Right Shoulder Down-Up";
				case MuscleType.RightShoulderFrontBack:
					return "Right Shoulder Front-Back";
				case MuscleType.RightArmDownUp:
					return "Right Arm Down-Up";
				case MuscleType.RightArmFrontBack:
					return "Right Arm Front-Back";
				case MuscleType.RightArmTwistInOut:
					return "Right Arm Twist In-Out";
				case MuscleType.RightForearmStretch:
					return "Right Forearm Stretch";
				case MuscleType.RightForearmTwistInOut:
					return "Right Forearm Twist In-Out";
				case MuscleType.RightHandDownUp:
					return "Right Hand Down-Up";
				case MuscleType.RightHandInOut:
					return "Right Hand In-Out";

				default:
					throw new ArgumentException(_this.ToString());
			}
		}
	}
}
