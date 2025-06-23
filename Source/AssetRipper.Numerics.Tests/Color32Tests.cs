using System.Drawing;
using System.Runtime.CompilerServices;

namespace AssetRipper.Numerics.Tests;

public class Color32Tests
{
	private Color32 _color;

	[SetUp]
	public void SetUp()
	{
		_color = new Color32(0x0A, 0x14, 0x1E, 0x28);
	}

	[Test]
	public void Color32_RgbaReturnsCorrectValue()
	{
		Assert.That(_color.Rgba, Is.EqualTo(0x281E140Au));
	}

	[Test]
	public void Color32_FromRgbaReturnsCorrectValue()
	{
		Assert.That(Color32.FromRgba(0x281E140A), Is.EqualTo(_color));
	}

	[Test]
	public void Color32_ExplicitColorFloatOperatorReturnsCorrectValue()
	{
		ColorFloat colorFloat = (ColorFloat)_color;
		Assert.That(colorFloat, Is.EqualTo(new ColorFloat(0.0392156877f, 0.0784313753f, 0.117647059f, 0.156862751f)));
	}

	[Test]
	public void Color32_ExplicitColor32OperatorReturnsCorrectValue()
	{
		Color32 color32 = (Color32)new ColorFloat(0.0392156877f, 0.0784313753f, 0.117647059f, 0.156862751f);
		Assert.That(color32, Is.EqualTo(new Color32(10, 20, 30, 40)));
	}

	[Test]
	public void Color32_BlackReturnsCorrectValue()
	{
		Assert.That(Color32.Black, Is.EqualTo(new Color32(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue)));
	}

	[Test]
	public void Color32_WhiteReturnsCorrectValue()
	{
		Assert.That(Color32.White, Is.EqualTo(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)));
	}

	[Test]
	public void RgbaPropertyMatchesShiftOperators()
	{
		Color32 color = new Color32(33, 57, 199, 255);
		uint value = unchecked((uint)(color.R | color.G << 8 | color.B << 16 | color.A << 24));
		Assert.That(color.Rgba, Is.EqualTo(value));
	}

	[Test]
	public void RgbaPropertyMatchesUnsafeAs()
	{
		Color32 color = new Color32(33, 57, 199, 255);
		uint value = Unsafe.As<Color32, uint>(ref color);
		Assert.That(color.Rgba, Is.EqualTo(value));
	}

	[Test]
	public void ConversionToSystemDrawingColorIsCorrect()
	{
		using (Assert.EnterMultipleScope())
		{
			Color color = (Color)_color;
			Assert.That(color.R, Is.EqualTo(_color.R));
			Assert.That(color.G, Is.EqualTo(_color.G));
			Assert.That(color.B, Is.EqualTo(_color.B));
			Assert.That(color.A, Is.EqualTo(_color.A));
		}
	}
}
