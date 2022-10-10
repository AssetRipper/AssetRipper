namespace AssetRipper.Core.Equality
{
	public static class ArrayEquality
	{
		public static bool AreEqual<T>(T first, T second) where T : IEquatable<T>
		{
			if (first is null || second is null)
			{
				return first is null && second is null;
			}
			return first.Equals(second);
		}

		public static bool AreEqualArrays<T>(T[] first, T[] second) where T : IEquatable<T>
		{
			if (first is null || second is null)
			{
				return first is null && second is null;
			}
			if (first.Length != second.Length)
			{
				return false;
			}
			for (int i = 0; i < first.Length; i++)
			{
				if (!AreEqual(first[i], second[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static bool AreEqualArrayArrays<T>(T[][] first, T[][] second) where T : IEquatable<T>
		{
			if (first is null || second is null)
			{
				return first is null && second is null;
			}
			if (first.Length != second.Length)
			{
				return false;
			}
			for (int i = 0; i < first.Length; i++)
			{
				if (!AreEqualArrays(first[i], second[i]))
				{
					return false;
				}
			}
			return true;
		}
	}
}
