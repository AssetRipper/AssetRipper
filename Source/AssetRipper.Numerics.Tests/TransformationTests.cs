namespace AssetRipper.Numerics.Tests;

internal class TransformationTests
{
	[Test]
	public void MoveThenScale()
	{
		Transformation transformation1 = new Transformation(new Vector3(0, .5f, 0), Quaternion.Identity, Vector3.One);
		Transformation transformation2 = new Transformation(Vector3.Zero, Quaternion.Identity, new Vector3(1, 2, 1));
		Transformation expectedResult = new Transformation(new Vector3(0, 1, 0), Quaternion.Identity, new Vector3(1, 2, 1));
		Assert.That(transformation1 * transformation2, Is.EqualTo(expectedResult));
	}

	[Test]
	public void MoveAndRotateThenScale()
	{
		Transformation transformation1 = new Transformation(new Vector3(0, .5f, 0), new Quaternion(0, 0, MathF.Sqrt(2) / 2, MathF.Sqrt(2) / 2), Vector3.One);
		Transformation transformation2 = new Transformation(Vector3.Zero, Quaternion.Identity, new Vector3(1, 2, 1));
		Transformation expectedResult = new Transformation(new Vector3(0, 1, 0), new Quaternion(0, 0, MathF.Sqrt(2) / 2, MathF.Sqrt(2) / 2), new Vector3(2, 1, 1));
		AssertApproximatelyEqual(transformation1 * transformation2, expectedResult, 0.00001f);
		Assert.That(!ApproximatelyEqual(transformation2 * transformation1, expectedResult, 0.00001f));
	}

	[Test]
	public void Identity()
	{
		Transformation actual = new Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One);
		AssertApproximatelyEqual(actual, Transformation.Identity, 0.00001f);
	}

	[Test]
	public void RemoveTranslation()
	{
		Vector3 translation = new Vector3(2, 5, -8);
		Quaternion rotation = Quaternion.CreateFromYawPitchRoll(2, 0.5f, 3);
		Vector3 scale = new Vector3(0.8f, 2, 3);
		Transformation withTranslation = Transformation.Create(translation, rotation, scale);
		Transformation withoutTranslation = Transformation.Create(Vector3.Zero, rotation, scale);
		AssertApproximatelyEqual(withTranslation.RemoveTranslation(), withoutTranslation, 0.00001f);
	}

	[Test]
	public void Inversion()
	{
		Transformation original = new Transformation(new Vector3(2, 5, -8), Quaternion.CreateFromYawPitchRoll(2, 0.5f, 3), new Vector3(0.8f, 2, 3));
		Transformation inverted = original.Invert();
		AssertApproximatelyEqual(original * inverted, Transformation.Identity, 0.00001f);
		AssertApproximatelyEqual(inverted * original, Transformation.Identity, 0.00001f);
	}

	[Test]
	public void InversionFromComponents()
	{
		Vector3 translation = new Vector3(2, 5, -8);
		Quaternion rotation = Quaternion.CreateFromYawPitchRoll(2, 0.5f, 3);
		Vector3 scale = new Vector3(0.8f, 2, 3);
		Transformation original = Transformation.Create(translation, rotation, scale);
		Transformation inverted = Transformation.CreateInverse(translation, rotation, scale);
		AssertApproximatelyEqual(original * inverted, Transformation.Identity, 0.00001f);
		AssertApproximatelyEqual(inverted * original, Transformation.Identity, 0.00001f);
	}

	[Test]
	public void InversionFromComponentsTranslation()
	{
		Vector3 translation = new Vector3(2, 5, -8);
		Quaternion rotation = Quaternion.Identity;
		Vector3 scale = Vector3.One;
		Transformation original = Transformation.Create(translation, rotation, scale);
		Transformation inverted = Transformation.CreateInverse(translation, rotation, scale);
		AssertApproximatelyEqual(original * inverted, Transformation.Identity, 0.00001f);
		AssertApproximatelyEqual(inverted * original, Transformation.Identity, 0.00001f);
	}

	[Test]
	public void InversionFromComponentsRotation()
	{
		Vector3 translation = Vector3.Zero;
		Quaternion rotation = Quaternion.CreateFromYawPitchRoll(2, 0.5f, 3);
		Vector3 scale = Vector3.One;
		Transformation original = Transformation.Create(translation, rotation, scale);
		Transformation inverted = Transformation.CreateInverse(translation, rotation, scale);
		AssertApproximatelyEqual(original * inverted, Transformation.Identity, 0.00001f);
		AssertApproximatelyEqual(inverted * original, Transformation.Identity, 0.00001f);
	}

	[Test]
	public void InversionFromComponentsScale()
	{
		Vector3 translation = Vector3.Zero;
		Quaternion rotation = Quaternion.Identity;
		Vector3 scale = new Vector3(0.8f, 2, 3);
		Transformation original = Transformation.Create(translation, rotation, scale);
		Transformation inverted = Transformation.CreateInverse(translation, rotation, scale);
		AssertApproximatelyEqual(original * inverted, Transformation.Identity, 0.00001f);
		AssertApproximatelyEqual(inverted * original, Transformation.Identity, 0.00001f);
	}

	private static void AssertApproximatelyEqual(Transformation actual, Transformation expected, float maxDeviation)
	{
		AssertApproximatelyEqual(actual.Matrix, expected.Matrix, maxDeviation);
	}

	private static void AssertApproximatelyEqual(Matrix4x4 actual, Matrix4x4 expected, float maxDeviation)
	{
		using (Assert.EnterMultipleScope())
		{
			AssertApproximatelyEqual(actual.M11, expected.M11, maxDeviation, nameof(Matrix4x4.M11));
			AssertApproximatelyEqual(actual.M12, expected.M12, maxDeviation, nameof(Matrix4x4.M12));
			AssertApproximatelyEqual(actual.M13, expected.M13, maxDeviation, nameof(Matrix4x4.M13));
			AssertApproximatelyEqual(actual.M14, expected.M14, maxDeviation, nameof(Matrix4x4.M14));
			AssertApproximatelyEqual(actual.M21, expected.M21, maxDeviation, nameof(Matrix4x4.M21));
			AssertApproximatelyEqual(actual.M22, expected.M22, maxDeviation, nameof(Matrix4x4.M22));
			AssertApproximatelyEqual(actual.M23, expected.M23, maxDeviation, nameof(Matrix4x4.M23));
			AssertApproximatelyEqual(actual.M24, expected.M24, maxDeviation, nameof(Matrix4x4.M24));
			AssertApproximatelyEqual(actual.M31, expected.M31, maxDeviation, nameof(Matrix4x4.M31));
			AssertApproximatelyEqual(actual.M32, expected.M32, maxDeviation, nameof(Matrix4x4.M32));
			AssertApproximatelyEqual(actual.M33, expected.M33, maxDeviation, nameof(Matrix4x4.M33));
			AssertApproximatelyEqual(actual.M34, expected.M34, maxDeviation, nameof(Matrix4x4.M34));
			AssertApproximatelyEqual(actual.M41, expected.M41, maxDeviation, nameof(Matrix4x4.M41));
			AssertApproximatelyEqual(actual.M42, expected.M42, maxDeviation, nameof(Matrix4x4.M42));
			AssertApproximatelyEqual(actual.M43, expected.M43, maxDeviation, nameof(Matrix4x4.M43));
			AssertApproximatelyEqual(actual.M44, expected.M44, maxDeviation, nameof(Matrix4x4.M44));
		}
	}

	private static void AssertApproximatelyEqual(float actual, float expected, float maxDeviation, string name)
	{
		if (ApproximatelyEqual(actual, expected, maxDeviation))
		{
			Assert.Fail($"{name}\nExpected: {expected}\nActual: {actual}");
		}
	}

	private static bool ApproximatelyEqual(Transformation actual, Transformation expected, float maxDeviation)
	{
		return ApproximatelyEqual(actual.Matrix, expected.Matrix, maxDeviation);
	}

	private static bool ApproximatelyEqual(Matrix4x4 actual, Matrix4x4 expected, float maxDeviation)
	{
		return ApproximatelyEqual(actual.M11, expected.M11, maxDeviation)
			&& ApproximatelyEqual(actual.M12, expected.M12, maxDeviation)
			&& ApproximatelyEqual(actual.M13, expected.M13, maxDeviation)
			&& ApproximatelyEqual(actual.M14, expected.M14, maxDeviation)
			&& ApproximatelyEqual(actual.M21, expected.M21, maxDeviation)
			&& ApproximatelyEqual(actual.M22, expected.M22, maxDeviation)
			&& ApproximatelyEqual(actual.M23, expected.M23, maxDeviation)
			&& ApproximatelyEqual(actual.M24, expected.M24, maxDeviation)
			&& ApproximatelyEqual(actual.M31, expected.M31, maxDeviation)
			&& ApproximatelyEqual(actual.M32, expected.M32, maxDeviation)
			&& ApproximatelyEqual(actual.M33, expected.M33, maxDeviation)
			&& ApproximatelyEqual(actual.M34, expected.M34, maxDeviation)
			&& ApproximatelyEqual(actual.M41, expected.M41, maxDeviation)
			&& ApproximatelyEqual(actual.M42, expected.M42, maxDeviation)
			&& ApproximatelyEqual(actual.M43, expected.M43, maxDeviation)
			&& ApproximatelyEqual(actual.M44, expected.M44, maxDeviation);
	}

	private static bool ApproximatelyEqual(float actual, float expected, float maxDeviation)
	{
		return float.Abs(actual - expected) > maxDeviation;
	}

	[Test]
	public void VectorMultiplicationTest()
	{
		//Arrange
		Vector3 vector = new Vector3(2, 4, 6);
		Transformation transformation = new Transformation(
			new Matrix4x4(2, 3, 4, 5,
						 6, 7, 8, 9,
						10, 11, 12, 13,
						14, 15, 16, 17));

		//Act
		Vector3 expected = Vector3.Transform(vector, transformation.Matrix);
		Vector3 actual = vector * transformation;

		//Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void MultiplyTransformationsTest()
	{
		//Arrange
		Transformation t1 = Transformation.Identity;
		Transformation t2 = Transformation.Create(
					new Vector3(5, 5, 5),
					new Quaternion(1, 1, 1, 1),
					new Vector3(2, 2, 2));

		//Act
		Transformation expected = new Transformation(
						new Matrix4x4(-6, 8, 0, 0,
									 0, -6, 8, 0,
									 8, 0, -6, 0,
									 5, 5, 5, 1));
		Transformation actual = t1 * t2;

		//Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void CreateInverseTest()
	{
		//Arrange
		Vector3 translation = new Vector3(1, 2, 3);
		Quaternion rotation = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f);
		Vector3 scale = new Vector3(2, 2, 2);

		//Act
		Transformation transform = Transformation.Create(translation, rotation, scale);
		Transformation inverse = transform.Invert();
		Matrix4x4 expected = Invert(transform.Matrix);
		Matrix4x4 actual = inverse.Matrix;

		//Assert
		Assert.That(actual, Is.EqualTo(expected));

		static Matrix4x4 Invert(Matrix4x4 matrix)
		{
			return Matrix4x4.Invert(matrix, out Matrix4x4 result) ? result : throw new System.Exception("Failed to invert matrix.");
		}
	}

	[Test]
	public void TransposeTest()
	{
		//Arrange
		Matrix4x4 matrix = new Matrix4x4(1, 2, 3, 4,
								   5, 6, 7, 8,
								   9, 10, 11, 12,
								   13, 14, 15, 16);
		Transformation transformation = new Transformation(matrix);

		//Act
		Transformation expected = new Transformation(Matrix4x4.Transpose(matrix));
		Transformation actual = transformation.Transpose();

		//Assert
		Assert.That(actual.Matrix, Is.EqualTo(expected.Matrix));
	}

	[Test]
	public void RemoveTranslationTest()
	{
		//Arrange
		Vector3 translation = new Vector3(1, 2, 3);
		Quaternion rotation = Quaternion.Identity;
		Vector3 scale = new Vector3(2, 2, 2);

		//Act
		Transformation transform = Transformation.Create(translation, rotation, scale);
		Transformation expected = new Transformation(Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation));
		Transformation actual = transform.RemoveTranslation();

		//Assert
		Assert.That(actual.Matrix, Is.EqualTo(expected.Matrix));
	}

	[Test]
	public void ConvertToMatrixTest()
	{
		//Arrange
		Vector3 translation = new Vector3(1, 2, 3);
		Quaternion rotation = new Quaternion(1, 2, 3, 4);
		Vector3 scale = new Vector3(2, 2, 2);

		//Act
		Transformation expected = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);
		Transformation actual = new Transformation(translation, rotation, scale);

		//Assert
		Assert.That(actual, Is.EqualTo(expected));
	}
}
