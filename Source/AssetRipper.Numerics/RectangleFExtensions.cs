using System.Drawing;

namespace AssetRipper.Numerics;

public static class RectangleFExtensions
{
	public static Vector2 Size(this RectangleF rectangle)
	{
		return rectangle.Size.ToVector2();
	}
}
