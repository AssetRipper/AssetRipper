using System.Collections.Generic;

namespace UtinyRipper
{
	public static class HashSetExtensions
	{
		public static void AddRange<T>(this HashSet<T> _this, IEnumerable<T> values)
		{
			foreach(T value in values)
			{
				_this.Add(value);
			}
		}
	}
}
