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
	public void UnpackedIntsAreTheSameAsTheOriginals()
	{
		PackedBitVector_Int32 packedVector = new();
		packedVector.PackUInts(ints);
		uint[] unpackedInts = packedVector.UnpackUInts();

		Assert.That(unpackedInts, Is.EqualTo(ints));
	}

	[Test]
	public void UnpackedShortsAreTheSameAsTheOriginals()
	{
		PackedBitVector_Int32 packedVector = new();
		packedVector.PackUShorts(shorts);
		ushort[] unpackedShorts = packedVector.UnpackUShorts();

		Assert.That(unpackedShorts, Is.EqualTo(shorts));
	}

	[Test]
	public void OldUnpackMethodIsConsistent()
	{
		PackedBitVector_Int32 packedVector = new();
		packedVector.PackUInts(ints);
		uint[] unpackedInts = packedVector.UnpackInts().Select(x => unchecked((uint)x)).ToArray();

		Assert.That(unpackedInts, Is.EqualTo(ints));
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
