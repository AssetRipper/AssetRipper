using System;

namespace uTinyRipper.Classes.Avatars
{
	public enum PhalangeType
	{
		Proximal		= 0,
		Intermediate	= 1,
		Distal			= 2,

		Last,
	}

	public static class PhalangeTypeExtensions
	{
		public static string ToAttributeString(this PhalangeType _this)
		{
			if (_this < PhalangeType.Last)
			{
				return _this.ToString();
			}
			throw new ArgumentException(_this.ToString());
		}
	}
}
