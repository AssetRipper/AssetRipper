using System;

namespace uTinyRipper
{
	public static class StringExtensions
	{
		public static bool Contains(this string _this, char c)
		{
			return _this.IndexOf(c) != -1;
		}
	}
}
