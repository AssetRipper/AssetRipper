using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Utils;
using System;
using System.Numerics;

namespace AssetRipper.Tests
{
	public class MathTests
	{
		[Test]
		public void TestNextPowerOfTwo()
		{
			Span<uint> testCases = stackalloc uint[] { 0, 1, 2, 3, 7, 15, 16, 17, 18, 19, 20, 21, 1023, 1024, 1025 };
			foreach (uint n in testCases)
			{
				uint pot = MathUtils.NextPowerOfTwo(n);
				Assert.IsTrue(BitOperations.IsPow2(pot));
				Assert.GreaterOrEqual(pot, n);
				if (n > 0)
				{
					Assert.Less(pot >> 1, n);
				}
			}
		}
	}
}
