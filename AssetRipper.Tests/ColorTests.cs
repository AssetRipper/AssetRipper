using AssetRipper.Core.Math.Colors;
using System.Runtime.CompilerServices;

namespace AssetRipper.Tests
{
	public class ColorTests
	{
		[Test]
		public void RgbaPropertyMatchesUnsafeAs()
		{
			Color32 color = new Color32(33, 57, 199, 255);
			uint value = Unsafe.As<Color32, uint>(ref color);
			Assert.AreEqual(value, color.Rgba);
		}
	}
}
