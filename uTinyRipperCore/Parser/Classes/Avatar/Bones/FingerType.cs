using System;

namespace uTinyRipper.Classes.Avatars
{
	public enum FingerType
	{
		Thumb		= 0,
		Index		= 1,
		Middle		= 2,
		Ring		= 3,
		Little		= 4,

		Last,
	}

	public static class FingerTypeExtensions
	{
		public static string ToAttributeString(this FingerType _this)
		{
			if (_this < FingerType.Last)
			{
				return _this.ToString();
			}
			throw new ArgumentException(_this.ToString());
		}
	}
}
