namespace AssemblyDumper
{
	public static class Assertions
	{
		public static void AssertEquality<T>(T? left, T? right) where T : IEquatable<T>
		{
			if (left == null && right == null)
				return;
			if(!left!.Equals(right))
			{
				throw new InvalidOperationException($"{left} was not equal to {right}");
			}
		}
	}
}
