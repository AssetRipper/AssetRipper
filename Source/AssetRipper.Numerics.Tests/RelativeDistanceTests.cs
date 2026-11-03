namespace AssetRipper.Numerics.Tests;

public class RelativeDistanceTests
{
	private readonly float[] randomFloats = MakeFloats(60);

	private static float[] MakeFloats(int count)
	{
		float[] result = new float[count];
		Random random = new(42);
		for (int i = 0; i < count; i++)
		{
			result[i] = 4 * random.NextSingle() - 2;//-2 to 2
		}
		return result;
	}

	[Test]
	public async Task ValuesOfOppositeSignsHaveDistanceOne()
	{
		await Assert.That(RelativeDistanceMethods.RelativeDistance(5f, -3f)).IsEqualTo(1f);
	}

	[Test]
	public async Task EqualValuesHaveDistanceZero()
	{

		{
			foreach (float value in randomFloats)
			{
				await AssertEqualZero(value);
			}
		}

		static async Task AssertEqualZero(float value)
		{
			float distance = RelativeDistanceMethods.RelativeDistance(value, value);
			await Assert.That(distance).IsZero().Because($"Value {value} did not have zero distance with itself.");
		}
	}

	[Test]
	public async Task DistanceIsNonnegativeAndSymmetric()
	{

		{
			for (int i = 0; i < randomFloats.Length - 1; i++)
			{
				float value1 = randomFloats[i];
				float value2 = randomFloats[i + 1];
				await AssertSymmetry(value1, value2);
			}
		}

		static async Task AssertSymmetry(float value1, float value2)
		{
			float distanceA = RelativeDistanceMethods.RelativeDistance(value1, value2);
			await Assert.That(distanceA).IsGreaterThanOrEqualTo(0f).Because($"Values {value1} and {value2} had negative distance.");
			float distanceB = RelativeDistanceMethods.RelativeDistance(value2, value1);
			await Assert.That(distanceA).IsEqualTo(distanceB).Because($"Values {value1} and {value2} did not exhibit symmetry.");
		}
	}

	[Test]
	public async Task TriangleInequalityHolds()
	{

		{
			for (int i = 0; i < randomFloats.Length - 2; i++)
			{
				float value1 = randomFloats[i];
				float value2 = randomFloats[i + 1];
				float value3 = randomFloats[i + 2];
				await AssertTriangleInequality(value1, value2, value3);
			}
		}

		static async Task AssertTriangleInequality(float value1, float value2, float value3)
		{
			float distanceA = RelativeDistanceMethods.RelativeDistance(value1, value2);
			float distanceB = RelativeDistanceMethods.RelativeDistance(value2, value3);
			float distanceC = RelativeDistanceMethods.RelativeDistance(value1, value3);
			await Assert.That(distanceA + distanceB).IsGreaterThanOrEqualTo(distanceC).Because($"Values {value1}, {value2}, and {value3} did not adhere to the triangle inequality.");
		}
	}

	[Test]
	public async Task DistanceIsTheSameInMultipleDimensions()
	{

		{
			for (int i = 0; i < randomFloats.Length - 1; i++)
			{
				float value1 = randomFloats[i];
				float value2 = randomFloats[i + 1];
				await AssertMultipleDimensionEquality(value1, value2);
			}
		}

		static async Task AssertMultipleDimensionEquality(float value1, float value2)
		{
			float distance1D = RelativeDistanceMethods.RelativeDistance(value1, value2);
			float distance2D = RelativeDistanceMethods.RelativeDistance(new Vector2(value1, 0), new Vector2(value2, 0));
			float distance3D = RelativeDistanceMethods.RelativeDistance(new Vector3(value1, 0, 0), new Vector3(value2, 0, 0));
			float distance4D = RelativeDistanceMethods.RelativeDistance(new Vector4(value1, 0, 0, 0), new Vector4(value2, 0, 0, 0));

			{
				await Assert.That(distance2D).IsEqualTo(distance1D).Because($"Values {value1} and {value2} did not have the same distance in 2 dimensions.");
				await Assert.That(distance3D).IsEqualTo(distance1D).Because($"Values {value1} and {value2} did not have the same distance in 3 dimensions.");
				await Assert.That(distance4D).IsEqualTo(distance1D).Because($"Values {value1} and {value2} did not have the same distance in 4 dimensions.");
			}
		}
	}

	[Test]
	public async Task ComparingZeroWithItselfIsZero()
	{

		{
			await Assert.That(RelativeDistanceMethods.RelativeDistance(0f, 0f)).IsZero().Because("1D distance between zero was not zero.");
			await Assert.That(RelativeDistanceMethods.RelativeDistance(Vector2.Zero, Vector2.Zero)).IsZero().Because("2D distance between zero was not zero.");
			await Assert.That(RelativeDistanceMethods.RelativeDistance(Vector3.Zero, Vector3.Zero)).IsZero().Because("3D distance between zero was not zero.");
			await Assert.That(RelativeDistanceMethods.RelativeDistance(Vector4.Zero, Vector4.Zero)).IsZero().Because("4D distance between zero was not zero.");
		}
	}

	[Test]
	public async Task SquaredDistanceBetweenEqualZeroValuesIsZero()
	{

		{
			await Assert.That(RelativeDistanceMethods.RelativeDistance2(0f, 0f)).IsZero();
			await Assert.That(RelativeDistanceMethods.RelativeDistance2(Vector2.Zero, Vector2.Zero)).IsZero();
		}
	}
}
