﻿using System;

namespace AssetRipper.Core.Classes.Misc.Bones
{
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
			switch (_this)
			{
				case ArmType.LeftHand:
					return BoneType.LeftHand;
				case ArmType.RightHand:
					return BoneType.RightHand;

				default:
					throw new ArgumentException(_this.ToString());
			}
		}
	}
}
