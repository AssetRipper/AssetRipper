using System;

namespace uTinyRipper.Classes.Avatars
{
	public enum TDoFBoneType
	{
		Spine			= 0,
		Chest			= 1,
		UpperChest		= 2,
		Neck			= 3,
		Head			= 4,
		LeftUpperLeg	= 5,
		LeftLowerLeg	= 6,
		LeftFoot		= 7,
		LeftToes		= 8,
		RightUpperLeg	= 9,
		RightLowerLeg	= 10,
		RightFoot		= 11,
		RightToes		= 12,
		LeftShoulder	= 13,
		LeftUpperArm	= 14,
		LeftLowerArm	= 15,
		LeftHand		= 16,
		RightShoulder	= 17,
		RightUpperArm	= 18,
		RightLowerArm	= 19,
		RightHand		= 20,

		Last,
	}

	public static class TDoFBoneTypeExtensions
	{
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsIncludeUpperChest(Version version)
		{
			return BoneTypeExtensions.IsIncludeUpperChest(version);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsIncludeHead(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsIncludeLeftLowerLeg(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsIncludeRightLowerLeg(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsIncludeLeftUpperArm(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsIncludeRightUpperArm(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}

		public static TDoFBoneType Update(this TDoFBoneType _this, Version version)
		{
			if (!IsIncludeUpperChest(version))
			{
				if (_this >= TDoFBoneType.UpperChest)
				{
					_this++;
				}
			}
			if (!IsIncludeHead(version))
			{
				if (_this >= TDoFBoneType.Head)
				{
					_this++;
				}
			}
			if (!IsIncludeLeftLowerLeg(version))
			{
				if (_this >= TDoFBoneType.LeftLowerLeg)
				{
					_this += 3;
				}
			}
			if (!IsIncludeRightLowerLeg(version))
			{
				if (_this >= TDoFBoneType.RightLowerLeg)
				{
					_this += 3;
				}
			}
			if (!IsIncludeLeftUpperArm(version))
			{
				if (_this >= TDoFBoneType.LeftUpperArm)
				{
					_this += 3;
				}
			}
			if (!IsIncludeRightUpperArm(version))
			{
				if (_this >= TDoFBoneType.RightUpperArm)
				{
					_this += 3;
				}
			}
			return _this;
		}
		
		public static BoneType ToBoneType(this TDoFBoneType _this)
		{
			switch (_this)
			{
				case TDoFBoneType.Spine:
					return BoneType.Spine;
				case TDoFBoneType.Chest:
					return BoneType.Chest;
				case TDoFBoneType.UpperChest:
					return BoneType.UpperChest;
				case TDoFBoneType.Neck:
					return BoneType.Neck;
				case TDoFBoneType.Head:
					return BoneType.Head;
				case TDoFBoneType.LeftUpperLeg:
					return BoneType.LeftUpperLeg;
				case TDoFBoneType.LeftLowerLeg:
					return BoneType.LeftLowerLeg;
				case TDoFBoneType.LeftFoot:
					return BoneType.LeftFoot;
				case TDoFBoneType.LeftToes:
					return BoneType.LeftToes;
				case TDoFBoneType.RightUpperLeg:
					return BoneType.RightUpperLeg;
				case TDoFBoneType.RightLowerLeg:
					return BoneType.RightLowerLeg;
				case TDoFBoneType.RightFoot:
					return BoneType.RightFoot;
				case TDoFBoneType.RightToes:
					return BoneType.RightToes;
				case TDoFBoneType.LeftShoulder:
					return BoneType.LeftShoulder;
				case TDoFBoneType.LeftUpperArm:
					return BoneType.LeftUpperArm;
				case TDoFBoneType.LeftLowerArm:
					return BoneType.LeftLowerArm;
				case TDoFBoneType.LeftHand:
					return BoneType.LeftHand;
				case TDoFBoneType.RightShoulder:
					return BoneType.RightShoulder;
				case TDoFBoneType.RightUpperArm:
					return BoneType.RightUpperArm;
				case TDoFBoneType.RightLowerArm:
					return BoneType.RightLowerArm;
				case TDoFBoneType.RightHand:
					return BoneType.RightHand;

				default:
					throw new ArgumentException(_this.ToString());
			}
		}
	}
}
