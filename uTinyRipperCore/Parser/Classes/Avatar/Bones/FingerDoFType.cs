using System;

namespace uTinyRipper.Classes.Avatars
{
	public enum FingerDoFType
	{
		_1Stretched		= 0,
		Spread			= 1,
		_2Stretched		= 2,
		_3Stretched		= 3,

		Last,
	}

	public static class FingerDoFTypeExtensions
	{
		public static string ToAttributeString(this FingerDoFType _this)
		{
			switch(_this)
			{
				case FingerDoFType._1Stretched:
					return "1 Stretched";
				case FingerDoFType.Spread:
					return "Spread";
				case FingerDoFType._2Stretched:
					return "2 Stretched";
				case FingerDoFType._3Stretched:
					return "3 Stretched";

				default:
					throw new ArgumentException(_this.ToString());
			}
		}
	}
}
