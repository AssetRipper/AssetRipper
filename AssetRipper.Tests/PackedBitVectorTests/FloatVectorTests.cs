using AssetRipper.Core.Equality;
using AssetRipper.Core.Math.PackedBitVectors;
using NUnit.Framework;
using System;

namespace AssetRipperTests.PackedBitVectorTests
{
	/// <summary>
	/// Tests for <see cref="PackedFloatVector"/>
	/// </summary>
	public class FloatVectorTests
	{
		private static readonly Random random = new Random(57089);
		private static readonly float[] floats = MakeFloats(20);

		private static float[] MakeFloats(int count)
		{
			float[] result = new float[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = 10f * random.NextSingle();
			}
			return result;
		}

		[Test]
		public void BitSize24ProducesHighlyAccurateResults()
		{
			PackedFloatVector packedVector = new PackedFloatVector();
			packedVector.PackFloats(floats, 24, false);
			float[] unpackedValues = packedVector.Unpack();
			AreAlmostEqual(floats, unpackedValues, 0.000001f);
		}

		[Test]
		public void BitSizeAdjustmentAlsoProducesUsableResults()
		{
			PackedFloatVector packedVector = new PackedFloatVector();
			packedVector.PackFloats(floats, 8, true);
			float[] unpackedValues = packedVector.Unpack();
			AreAlmostEqual(floats, unpackedValues, 0.01f);
		}

		private static void AreAlmostEqual(float[] expected, float[] actual, float maxDeviation)
		{
			if (expected.Length != actual.Length)
				Assert.Fail($"Lengths were inequal.\nExpected: {expected.Length}\nBut was: {actual.Length}");

			for (int i = 0; i < expected.Length; i++)
			{
				if(!NearEquality.AlmostEqualByDeviation(expected[i], actual[i], maxDeviation))
				{
					Assert.Fail($"Values significantly differ at index {i}\nExpected: {expected[i]}\nBut was: {actual[i]}");
				}
			}
		}
	}
}
