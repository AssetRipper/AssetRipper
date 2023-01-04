using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.PackedBitVector_Single;

namespace AssetRipper.Tests.PackedBitVectorTests
{
	/// <summary>
	/// Tests for <see cref="PackedBitVector_Single"/>
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
			PackedBitVector_Single packedVector = new();
			packedVector.PackFloats(floats, 24, false);
			float[] unpackedValues = packedVector.Unpack();
			Assert.That(unpackedValues, Is.EqualTo(floats).Within(0.000001f));
		}

		[Test]
		public void BitSizeAdjustmentAlsoProducesUsableResults()
		{
			PackedBitVector_Single packedVector = new();
			packedVector.PackFloats(floats, 8, true);
			float[] unpackedValues = packedVector.Unpack();
			Assert.That(unpackedValues, Is.EqualTo(floats).Within(0.01f));
		}
	}
}
