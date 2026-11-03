namespace AssetRipper.Numerics.Tests;

public class ColorFloatTests
{
	[Test]
	public async Task ColorFloat_Addition_ReturnsExpectedResult()
	{
		ColorFloat color1 = new ColorFloat(0.1f, 0.2f, 0.3f, 0.4f);
		ColorFloat color2 = new ColorFloat(0.5f, 0.6f, 0.7f, 0.8f);
		ColorFloat expectedResult = new ColorFloat(0.6f, 0.8f, 1.0f, 1.2f);

		ColorFloat actualResult = color1 + color2;

		await Assert.That(actualResult).IsEqualTo(expectedResult);
	}

	[Test]
	public async Task ColorFloat_Subtraction_ReturnsExpectedResult()
	{
		ColorFloat color1 = new ColorFloat(5.0f, 6.0f, 7.0f, 8.0f);
		ColorFloat color2 = new ColorFloat(1.0f, 2.0f, 3.0f, 4.0f);
		ColorFloat expectedResult = new ColorFloat(4.0f, 4.0f, 4.0f, 4.0f);

		ColorFloat actualResult = color1 - color2;

		await Assert.That(actualResult).IsEqualTo(expectedResult);
	}

	[Test]
	public async Task ColorFloat_Clamp_ReturnsExpectedResult()
	{
		ColorFloat color1 = new ColorFloat(-0.1f, 1.2f, 0.3f, -1.3f);
		ColorFloat expectedResult = new ColorFloat(0.0f, 1.0f, 0.3f, 0.0f);

		ColorFloat actualResult = color1.Clamp();

		await Assert.That(actualResult).IsEqualTo(expectedResult);
	}

	[Test]
	public async Task ColorFloat_Multiplication_ReturnsExpectedResult()
	{
		ColorFloat color1 = new ColorFloat(0.1f, 0.2f, 0.3f, 0.4f);
		float factor = 2.0f;
		ColorFloat expectedResult = new ColorFloat(0.2f, 0.4f, 0.6f, 0.8f);

		ColorFloat actualResult = color1 * factor;

		await Assert.That(actualResult).IsEqualTo(expectedResult);
	}

	[Test]
	public async Task ColorFloat_StaticMethods_BlackAndWhiteTest_ReturnsExpectedResult()
	{
		ColorFloat black = ColorFloat.Black;
		ColorFloat white = ColorFloat.White;

		await Assert.That(black).IsEqualTo(new ColorFloat(0, 0, 0, 1));
		await Assert.That(white).IsEqualTo(new ColorFloat(1, 1, 1, 1));
	}
}