namespace AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.Bones;

public enum ArmType
{
	LeftHand = 0,
	RightHand = 1,

	Last,
}

public static class ArmTypeExtensions
{
	public static BoneType ToBoneType(this ArmType _this)
	{
		return _this switch
		{
			ArmType.LeftHand => BoneType.LeftHand,
			ArmType.RightHand => BoneType.RightHand,
			_ => throw new ArgumentException(_this.ToString()),
		};
	}
}
