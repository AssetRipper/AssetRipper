namespace AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.Bones;

public enum TDoFBoneType
{
	Spine = 0,
	Chest = 1,
	UpperChest = 2,
	Neck = 3,
	Head = 4,
	LeftUpperLeg = 5,
	LeftLowerLeg = 6,
	LeftFoot = 7,
	LeftToes = 8,
	RightUpperLeg = 9,
	RightLowerLeg = 10,
	RightFoot = 11,
	RightToes = 12,
	LeftShoulder = 13,
	LeftUpperArm = 14,
	LeftLowerArm = 15,
	LeftHand = 16,
	RightShoulder = 17,
	RightUpperArm = 18,
	RightLowerArm = 19,
	RightHand = 20,

	Last,
}

public static class TDoFBoneTypeExtensions
{
	/// <summary>
	/// 5.6.0 and greater
	/// </summary>
	public static bool IsIncludeUpperChest(UnityVersion version) => BoneTypeExtensions.IsIncludeUpperChest(version);
	/// <summary>
	/// 2017.3 and greater
	/// </summary>
	public static bool IsIncludeHead(UnityVersion version) => version.GreaterThanOrEquals(2017, 3);
	/// <summary>
	/// 2017.3 and greater
	/// </summary>
	public static bool IsIncludeLeftLowerLeg(UnityVersion version) => version.GreaterThanOrEquals(2017, 3);
	/// <summary>
	/// 2017.3 and greater
	/// </summary>
	public static bool IsIncludeRightLowerLeg(UnityVersion version) => version.GreaterThanOrEquals(2017, 3);
	/// <summary>
	/// 2017.3 and greater
	/// </summary>
	public static bool IsIncludeLeftUpperArm(UnityVersion version) => version.GreaterThanOrEquals(2017, 3);
	/// <summary>
	/// 2017.3 and greater
	/// </summary>
	public static bool IsIncludeRightUpperArm(UnityVersion version) => version.GreaterThanOrEquals(2017, 3);

	public static TDoFBoneType Update(this TDoFBoneType _this, UnityVersion version)
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
		return _this switch
		{
			TDoFBoneType.Spine => BoneType.Spine,
			TDoFBoneType.Chest => BoneType.Chest,
			TDoFBoneType.UpperChest => BoneType.UpperChest,
			TDoFBoneType.Neck => BoneType.Neck,
			TDoFBoneType.Head => BoneType.Head,
			TDoFBoneType.LeftUpperLeg => BoneType.LeftUpperLeg,
			TDoFBoneType.LeftLowerLeg => BoneType.LeftLowerLeg,
			TDoFBoneType.LeftFoot => BoneType.LeftFoot,
			TDoFBoneType.LeftToes => BoneType.LeftToes,
			TDoFBoneType.RightUpperLeg => BoneType.RightUpperLeg,
			TDoFBoneType.RightLowerLeg => BoneType.RightLowerLeg,
			TDoFBoneType.RightFoot => BoneType.RightFoot,
			TDoFBoneType.RightToes => BoneType.RightToes,
			TDoFBoneType.LeftShoulder => BoneType.LeftShoulder,
			TDoFBoneType.LeftUpperArm => BoneType.LeftUpperArm,
			TDoFBoneType.LeftLowerArm => BoneType.LeftLowerArm,
			TDoFBoneType.LeftHand => BoneType.LeftHand,
			TDoFBoneType.RightShoulder => BoneType.RightShoulder,
			TDoFBoneType.RightUpperArm => BoneType.RightUpperArm,
			TDoFBoneType.RightLowerArm => BoneType.RightLowerArm,
			TDoFBoneType.RightHand => BoneType.RightHand,
			_ => throw new ArgumentException(_this.ToString()),
		};
	}
}
