using System;

namespace uTinyRipper.Classes.Avatars
{
	public enum LimbType
	{
		LeftFoot	= 0,
		RightFoot	= 1,
		LeftHand	= 2,
		RightHand	= 3,

		Last,
	}
	
	public static class LimbTypeExtensions
	{
		public static BoneType ToBoneType(this LimbType _this)
		{
			switch (_this)
			{
				case LimbType.LeftFoot:
					return BoneType.LeftFoot;
				case LimbType.RightFoot:
					return BoneType.RightFoot;
				case LimbType.LeftHand:
					return BoneType.LeftHand;
				case LimbType.RightHand:
					return BoneType.RightHand;

				default:
					throw new ArgumentException(_this.ToString());
			}
		}
	}
}
