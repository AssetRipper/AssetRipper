using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Utils
{
	public static class ArrayUtils
	{
		public static T[] Combine<T>(T[] array1, T[] array2)
		{
			if (array1 == null) throw new ArgumentNullException(nameof(array1));
			if (array2 == null) throw new ArgumentNullException(nameof(array2));

			List<T> result = new List<T>(array1.Length + array2.Length);
			foreach (var item1 in array1)
				result.Add(item1);
			foreach (var item2 in array2)
				result.Add(item2);
			return result.ToArray();
		}
	}
}
