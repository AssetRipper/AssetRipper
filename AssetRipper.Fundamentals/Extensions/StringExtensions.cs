using System;

namespace AssetRipper.Core.Extensions
{
	public static class StringExtensions
	{
		public static bool Contains(this string _this, char c)
		{
			return _this.IndexOf(c) != -1;
		}

		public static string[] SplitLines(this string _this)
		{
			return _this.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		}
	}
}
