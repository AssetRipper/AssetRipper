using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.PackedBitVector_Single;

namespace AssetRipper.Tests.PackedBitVectorTests;

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
	public async Task BitSize24ProducesHighlyAccurateResults()
	{
		PackedBitVector_Single packedVector = new();
		packedVector.Pack(floats, 24, false);
		float[] unpackedValues = packedVector.Unpack();
		await AssertEqualWithin(unpackedValues, floats, 0.000001f);
	}

	[Test]
	public async Task BitSizeAdjustmentAlsoProducesUsableResults()
	{
		PackedBitVector_Single packedVector = new();
		packedVector.Pack(floats, 8, true);
		float[] unpackedValues = packedVector.Unpack();
		await AssertEqualWithin(unpackedValues, floats, 0.01f);
	}

	[Test]
	[MatrixDataSource]
	public async Task PackingAndUnpackingDoesNotThrowAndGivesCorrectLength([MatrixRange<int>(1, 32)] int bitSize)
	{
		PackedBitVector_Single packedVector = new();
		packedVector.Pack(floats, bitSize, false);
		float[] unpackedValues = packedVector.Unpack();
		await Assert.That(unpackedValues.Length).IsEqualTo(floats.Length);
	}

	[Test]
	public async Task DifferingChunkStrideAndChunkSizeWorks()
	{
		await Assert.That(floats.Length % 4 == 0).IsTrue();
		PackedBitVector_Single packedVector = new();
		packedVector.Pack(floats);
		float[] actual = packedVector.Unpack(3, 4);
		float[] expected = new float[floats.Length / 4 * 3];
		for (int chunk = 0; chunk < floats.Length / 4; chunk++)
		{
			for (int i = 0; i < 3; i++)
			{
				expected[chunk * 3 + i] = floats[chunk * 4 + i];
			}
		}
		await AssertEqualWithin(actual, expected, 0.000001f);
	}

	private static async Task AssertEqualWithin(float[] actual, float[] expected, float tolerance)
	{
		await Assert.That(actual.Length).IsEqualTo(expected.Length);
		for (int i = 0; i < actual.Length; i++)
		{
			await Assert.That(MathF.Abs(actual[i] - expected[i]) <= tolerance).IsTrue()
				.Because($"Values at index {i} differ by more than {tolerance}.");
		}
	}
}