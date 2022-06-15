using AssetRipper.Core.Math.PackedBitVectors;
using NUnit.Framework;
using System;
using System.Linq;

namespace AssetRipperTests.PackedBitVectorTests
{
	/// <summary>
	/// Tests for <see cref="PackedIntVector"/>
	/// </summary>
	public class IntVectorTests
	{
		private static readonly Random random = new Random(57089);
		private static readonly uint[] ints = MakeInts(20);
		private static readonly ushort[] shorts = MakeShorts(20);

		[Test]
		public void UnpackedIntsAreTheSameAsTheOriginals()
		{
			PackedIntVector packedVector = new PackedIntVector();
			packedVector.PackUInts(ints);
			uint[] unpackedInts = packedVector.UnpackUInts();

			Assert.AreEqual(ints, unpackedInts);
		}

		[Test]
		public void UnpackedShortsAreTheSameAsTheOriginals()
		{
			PackedIntVector packedVector = new PackedIntVector();
			packedVector.PackUShorts(shorts);
			ushort[] unpackedShorts = packedVector.UnpackUShorts();

			Assert.AreEqual(shorts, unpackedShorts);
		}

		[Test]
		public void OldUnpackMethodIsConsistent()
		{
			PackedIntVector packedVector = new PackedIntVector();
			packedVector.PackUInts(ints);
			uint[] unpackedInts = packedVector.UnpackInts().Select(x => unchecked((uint)x)).ToArray();

			Assert.AreEqual(ints, unpackedInts);
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
}
