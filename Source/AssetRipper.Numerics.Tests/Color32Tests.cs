using System.Drawing;
using System.Runtime.CompilerServices;

namespace AssetRipper.Numerics.Tests;

public class Color32Tests
{
	private Color32 _color;

	[Before(HookType.Test)]
	public void SetUp()
	{
		_color = new Color32(0x0A, 0x14, 0x1E, 0x28);
	}

	[Test]
	public async Task Color32_RgbaReturnsCorrectValue()
	{
		await Assert.That(_color.Rgba).IsEqualTo(0x281E140Au);
	}

	[Test]
	public async Task Color32_FromRgbaReturnsCorrectValue()
	{
		await Assert.That(Color32.FromRgba(0x281E140A)).IsEqualTo(_color);
	}

	[Test]
	public async Task Color32_ExplicitColorFloatOperatorReturnsCorrectValue()
	{
		ColorFloat colorFloat = (ColorFloat)_color;
		await Assert.That(colorFloat).IsEqualTo(new ColorFloat(0.0392156877f, 0.0784313753f, 0.117647059f, 0.156862751f));
	}

	[Test]
	public async Task Color32_ExplicitColor32OperatorReturnsCorrectValue()
	{
		Color32 color32 = (Color32)new ColorFloat(0.0392156877f, 0.0784313753f, 0.117647059f, 0.156862751f);
		await Assert.That(color32).IsEqualTo(new Color32(10, 20, 30, 40));
	}

	[Test]
	public async Task Color32_BlackReturnsCorrectValue()
	{
		await Assert.That(Color32.Black).IsEqualTo(new Color32(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue));
	}

	[Test]
	public async Task Color32_WhiteReturnsCorrectValue()
	{
		await Assert.That(Color32.White).IsEqualTo(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
	}

	[Test]
	public async Task RgbaPropertyMatchesShiftOperators()
	{
		Color32 color = new Color32(33, 57, 199, 255);
		uint value = unchecked((uint)(color.R | color.G << 8 | color.B << 16 | color.A << 24));
		await Assert.That(color.Rgba).IsEqualTo(value);
	}

	[Test]
	public async Task RgbaPropertyMatchesUnsafeAs()
	{
		Color32 color = new Color32(33, 57, 199, 255);
		uint value = Unsafe.As<Color32, uint>(ref color);
		await Assert.That(color.Rgba).IsEqualTo(value);
	}

	[Test]
	public async Task ConversionToSystemDrawingColorIsCorrect()
	{

		{
			Color color = (Color)_color;
			await Assert.That(color.R).IsEqualTo(_color.R);
			await Assert.That(color.G).IsEqualTo(_color.G);
			await Assert.That(color.B).IsEqualTo(_color.B);
			await Assert.That(color.A).IsEqualTo(_color.A);
		}
	}
}