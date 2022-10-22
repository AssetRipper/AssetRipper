namespace AssetRipper.Numerics.Tests
{
	public class RelativeDistanceTests
	{
		private static readonly Random random = new Random(12345);
		private static readonly float[] randomFloats = MakeFloats(60);

		private static float[] MakeFloats(int count)
		{
			float[] result = new float[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = 4 * random.NextSingle() - 2;//-2 to 2
			}
			return result;
		}

		[Test]
		public void ValuesOfOppositeSignsHaveDistanceOne()
		{
			Assert.That(RelativeDistanceMethods.RelativeDistance(5f, -3f), Is.EqualTo(1f));
		}

		[Test]
		public void EqualValuesHaveDistanceZero()
		{
			Assert.Multiple(() =>
			{
				foreach (float value in randomFloats)
				{
					AssertEqualZero(value);
				}
			});

			static void AssertEqualZero(float value)
			{
				float distance = RelativeDistanceMethods.RelativeDistance(value, value);
				Assert.That(distance, Is.EqualTo(0f), () => $"Value {value} did not have zero distance with itself.");
			}
		}

		[Test]
		public void DistanceIsNonnegativeAndSymmetric()
		{
			Assert.Multiple(() =>
			{
				for (int i = 0; i < randomFloats.Length - 1; i++)
				{
					float value1 = randomFloats[i];
					float value2 = randomFloats[i + 1];
					AssertSymmetry(value1, value2);
				}
			});

			static void AssertSymmetry(float value1, float value2)
			{
				float distanceA = RelativeDistanceMethods.RelativeDistance(value1, value2);
				Assert.That(distanceA, Is.GreaterThanOrEqualTo(0f), () => $"Values {value1} and {value2} had negative distance.");
				float distanceB = RelativeDistanceMethods.RelativeDistance(value2, value1);
				Assert.That(distanceA, Is.EqualTo(distanceB), () => $"Values {value1} and {value2} did not exhibit symmetry.");
			}
		}

		[Test]
		public void TriangleInequalityHolds()
		{
			Assert.Multiple(() =>
			{
				for (int i = 0; i < randomFloats.Length - 2; i++)
				{
					float value1 = randomFloats[i];
					float value2 = randomFloats[i + 1];
					float value3 = randomFloats[i + 2];
					AssertTriangleInequality(value1, value2, value3);
				}
			});

			static void AssertTriangleInequality(float value1, float value2, float value3)
			{
				float distanceA = RelativeDistanceMethods.RelativeDistance(value1, value2);
				float distanceB = RelativeDistanceMethods.RelativeDistance(value2, value3);
				float distanceC = RelativeDistanceMethods.RelativeDistance(value1, value3);
				Assert.That(distanceA + distanceB, Is.GreaterThanOrEqualTo(distanceC), () => $"Values {value1}, {value2}, and {value3} did not adhere to the triangle inequality.");
			}
		}

		[Test]
		public void DistanceIsTheSameInMultipleDimensions()
		{
			Assert.Multiple(() =>
			{
				for (int i = 0; i < randomFloats.Length - 1; i++)
				{
					float value1 = randomFloats[i];
					float value2 = randomFloats[i + 1];
					AssertMultipleDimensionEquality(value1, value2);
				}
			});

			static void AssertMultipleDimensionEquality(float value1, float value2)
			{
				float distance1D = RelativeDistanceMethods.RelativeDistance(value1, value2);
				float distance2D = RelativeDistanceMethods.RelativeDistance(new Vector2(value1, 0), new Vector2(value2, 0));
				float distance3D = RelativeDistanceMethods.RelativeDistance(new Vector3(value1, 0, 0), new Vector3(value2, 0, 0));
				float distance4D = RelativeDistanceMethods.RelativeDistance(new Vector4(value1, 0, 0, 0), new Vector4(value2, 0, 0, 0));
				Assert.Multiple(() =>
				{
					Assert.That(distance2D, Is.EqualTo(distance1D), () => $"Values {value1} and {value2} did not have the same distance in 2 dimensions.");
					Assert.That(distance3D, Is.EqualTo(distance1D), () => $"Values {value1} and {value2} did not have the same distance in 3 dimensions.");
					Assert.That(distance4D, Is.EqualTo(distance1D), () => $"Values {value1} and {value2} did not have the same distance in 4 dimensions.");
				});
			}
		}

		[Test]
		public void ComparingZeroWithItselfIsZero()
		{
			Assert.Multiple(() =>
			{
				Assert.That(RelativeDistanceMethods.RelativeDistance(0f, 0f), Is.EqualTo(0f), () => $"1D distance between zero was not zero.");
				Assert.That(RelativeDistanceMethods.RelativeDistance(Vector2.Zero, Vector2.Zero), Is.EqualTo(0f), () => $"2D distance between zero was not zero.");
				Assert.That(RelativeDistanceMethods.RelativeDistance(Vector3.Zero, Vector3.Zero), Is.EqualTo(0f), () => $"3D distance between zero was not zero.");
				Assert.That(RelativeDistanceMethods.RelativeDistance(Vector4.Zero, Vector4.Zero), Is.EqualTo(0f), () => $"4D distance between zero was not zero.");
			});
		}
	}
}
