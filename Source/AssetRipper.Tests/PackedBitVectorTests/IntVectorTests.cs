using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.PackedBitVector_Int32;

namespace AssetRipper.Tests.PackedBitVectorTests;

/// <summary>
/// Tests for <see cref="PackedBitVector_Int32"/>
/// </summary>
public class IntVectorTests
{
	private static readonly Random random = new Random(57089);
	private static readonly uint[] ints = MakeInts(20);
	private static readonly ushort[] shorts = MakeShorts(20);

	[Test]
	public async Task UnpackedIntsAreTheSameAsTheOriginals()
	{
		PackedBitVector_Int32 packedVector = new();
		packedVector.PackUInts(ints);
		uint[] unpackedInts = packedVector.UnpackUInts();

		await Assert.That(unpackedInts.SequenceEqual(ints)).IsTrue();
	}

	[Test]
	public async Task UnpackedShortsAreTheSameAsTheOriginals()
	{
		PackedBitVector_Int32 packedVector = new();
		packedVector.PackUShorts(shorts);
		ushort[] unpackedShorts = packedVector.UnpackUShorts();

		await Assert.That(unpackedShorts.SequenceEqual(shorts)).IsTrue();
	}

	[Test]
	public async Task OldUnpackMethodIsConsistent()
	{
		PackedBitVector_Int32 packedVector = new();
		packedVector.PackUInts(ints);
		uint[] unpackedInts = packedVector.UnpackInts().Select(x => unchecked((uint)x)).ToArray();

		await Assert.That(unpackedInts.SequenceEqual(ints)).IsTrue();
	}

	private static uint[] MakeInts(int count)
	{
		uint[] result = new uint[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = unchecked((uint)random.Next());
		}
		return result;
	}

	private static ushort[] MakeShorts(int count)
	{
		ushort[] result = new ushort[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = unchecked((ushort)random.Next(ushort.MaxValue));
		}
		return result;
	}
}