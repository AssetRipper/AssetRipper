namespace AssetRipper.Numerics.Tests
{
	public class ColorTests
	{
		[Test]
		public void RgbaPropertyMatchesUnsafeAs()
		{
			Color32 color = new Color32(33, 57, 199, 255);
			uint value = unchecked((uint)(color.R | color.G << 8 | color.B << 16 | color.A << 24));
			Assert.That(color.Rgba, Is.EqualTo(value));
		}
	}
}
