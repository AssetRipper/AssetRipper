using System.Collections.Generic;

namespace uTinyRipper
{
	public class SplitNameComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			int xNumber = GetSplitIndex(x);
			int yNumber = GetSplitIndex(y);
			return xNumber.CompareTo(yNumber);
		}

		private static int GetSplitIndex(string value)
		{
			int i;
			for (i = value.Length - 1; i >= 0; i--)
			{
				if (!char.IsDigit(value[i]))
				{
					i++;
					break;
				}
			}
			string number = value.Substring(i);
			return int.Parse(number);
		}
	}
}
