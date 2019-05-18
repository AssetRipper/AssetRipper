using System;

namespace uTinyRipper
{
	public static class StringExtensions
	{
		public static bool Contains(this string _this, char c)
		{
			return _this.IndexOf(c) != -1;
		}

		public static bool Contains(this string _this, string subString, StringComparison comparisonType)
		{
			return _this.IndexOf(subString, comparisonType) != -1;
		}
	}
}
