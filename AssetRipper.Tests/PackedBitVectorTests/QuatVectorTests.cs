using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Subclasses.PackedBitVector_Quaternionf;
using System;

namespace AssetRipperTests.PackedBitVectorTests
{
	/// <summary>
	/// Tests for <see cref="PackedBitVector_Quaternionf"/>
	/// </summary>
	public class QuatVectorTests
	{
		private static readonly Random random = new Random(57089);
		private static readonly Quaternionf[] quaternions = MakeQuaternions(20);

		private static Quaternionf[] MakeQuaternions(int count)
		{
			Quaternionf[] result = new Quaternionf[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = GetRandomQuaternion();
			}
			return result;
		}

		private static Quaternionf GetRandomQuaternion() => GetRandomEuler().ToQuaternion(false);

		private static Vector3f GetRandomEuler()
		{
			return new Vector3f(GetRandomAngle(), GetRandomAngle(), GetRandomAngle());
		}

		private static float GetRandomAngle()
		{
			return (float)(random.NextDouble() * 2d * Math.PI);
		}

		[Test]
		public void PackingAndUnpackingGiveTheSameValues()
		{
			PackedBitVector_Quaternionf packedVector = new();
			packedVector.Pack(quaternions);
			Quaternionf[] unpackedQuaternions = packedVector.Unpack();

			for (int i = 0; i < quaternions.Length; i++)
			{
				bool equal = quaternions[i].IsEqualUsingDot(unpackedQuaternions[i]);
				if (!equal)
				{
					throw new Exception($"Index {i}: Original: {quaternions[i]} Unpacked: {unpackedQuaternions[i]} Dot: {quaternions[i].Dot(unpackedQuaternions[i])}");
				}
			}
		}
	}
}
