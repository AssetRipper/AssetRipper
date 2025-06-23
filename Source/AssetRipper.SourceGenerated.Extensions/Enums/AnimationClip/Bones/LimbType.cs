namespace AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.Bones;

public enum LimbType
{
	LeftFoot = 0,
	RightFoot = 1,
	LeftHand = 2,
	RightHand = 3,

	Last,
}

public static class LimbTypeExtensions
{
	public static BoneType ToBoneType(this LimbType _this)
	{
		return _this switch
		{
			LimbType.LeftFoot => BoneType.LeftFoot,
			LimbType.RightFoot => BoneType.RightFoot,
			LimbType.LeftHand => BoneType.LeftHand,
			LimbType.RightHand => BoneType.RightHand,
			_ => throw new ArgumentException(_this.ToString()),
		};
	}
}
