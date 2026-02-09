using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.PackedBitVector_Quaternionf;
using System.Numerics;

namespace AssetRipper.Tests.PackedBitVectorTests;

/// <summary>
/// Tests for <see cref="PackedBitVector_Quaternionf"/>
/// </summary>
public class QuatVectorTests
{
	private static readonly Random random = new Random(57089);
	private static readonly Quaternion[] quaternions = MakeQuaternions(20);

	private static Quaternion[] MakeQuaternions(int count)
	{
		Quaternion[] result = new Quaternion[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = GetRandomQuaternion();
		}
		return result;
	}

	private static Quaternion GetRandomQuaternion() => GetRandomEuler().ToQuaternion(false);

	private static Vector3 GetRandomEuler()
	{
		return new Vector3(GetRandomAngle(), GetRandomAngle(), GetRandomAngle());
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
		Quaternion[] unpackedQuaternions = packedVector.Unpack();

		for (int i = 0; i < quaternions.Length; i++)
		{
			bool equal = quaternions[i].EqualsByDot(unpackedQuaternions[i]);
			if (!equal)
			{
				throw new Exception($"Index {i}: Original: {quaternions[i]} Unpacked: {unpackedQuaternions[i]} Dot: {quaternions[i].Dot(unpackedQuaternions[i])}");
			}
		}
	}
}
