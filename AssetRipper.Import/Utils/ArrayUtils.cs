namespace AssetRipper.Core.Utils
{
	public static class ArrayUtils
	{
		public static T[] Combine<T>(T[] array1, T[] array2)
		{
			if (array1 == null)
			{
				throw new ArgumentNullException(nameof(array1));
			}

			if (array2 == null)
			{
				throw new ArgumentNullException(nameof(array2));
			}

			T[] result = new T[array1.Length + array2.Length];
			for (int i = 0; i < array1.Length; i++)
			{
				result[i] = array1[i];
			}
			for (int j = 0; j < array2.Length; j++)
			{
				result[j + array1.Length] = array2[j];
			}
			return result;
		}

		/// <summary>
		/// Creates an array with non-null elements
		/// </summary>
		/// <typeparam name="T">The type of the array elements</typeparam>
		/// <param name="length">The length of the array</param>
		/// <returns>A new array of the specified length and type</returns>
		/// <exception cref="ArgumentOutOfRangeException">Length less than zero</exception>
		public static T[] CreateAndInitializeArray<T>(int length) where T : new()
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(length));
			}

			if (length == 0)
			{
				return Array.Empty<T>();
			}

			T[] array = new T[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = new();
			}
			return array;
		}
	}
}
