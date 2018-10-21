using System;

namespace uTinyRipper.Classes.Avatars
{
	public enum BoneType
	{
		Hips			= 0,
		LeftUpperLeg	= 1,
		RightUpperLeg	= 2,
		LeftLowerLeg	= 3,
		RightLowerLeg	= 4,
		LeftFoot		= 5,
		RightFoot		= 6,
		Spine			= 7,
		Chest			= 8,
		UpperChest		= 9,
		Neck			= 10,
		Head			= 11,
		LeftShoulder	= 12,
		RightShoulder	= 13,
		LeftUpperArm	= 14,
		RightUpperArm	= 15,
		LeftLowerArm	= 16,
		RightLowerArm	= 17,
		LeftHand		= 18,
		RightHand		= 19,
		LeftToes		= 20,
		RightToes		= 21,
		LeftEye			= 22,
		RightEye		= 23,
		Jaw				= 24,

		Last,
	}

	public static class BoneTypeExtensions
	{
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsIncludeUpperChest(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		public static BoneType Update(this BoneType _this, Version version)
		{
			if(!IsIncludeUpperChest(version))
			{
				if(_this >= BoneType.UpperChest)
				{
					_this++;
				}
			}
			return _this;
		}

		public static string ToAttributeString(this BoneType _this)
		{
			if(_this < BoneType.Last)
			{
				return _this.ToString();
			}
			throw new ArgumentException(_this.ToString());
		}
	}
}
