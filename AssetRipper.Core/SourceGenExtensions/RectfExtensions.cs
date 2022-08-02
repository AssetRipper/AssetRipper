using AssetRipper.Core.Math.Vectors;
using AssetRipper.SourceGenerated.Subclasses.Rectf;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class RectfExtensions
	{
		public static Vector2f Center(this IRectf rectangle) => new Vector2f(rectangle.X + (rectangle.Width / 2.0f), rectangle.Y + (rectangle.Height / 2.0f));
		public static Vector2f Position(this IRectf rectangle) => new Vector2f(rectangle.X, rectangle.Y);
		public static Vector2f Size(this IRectf rectangle) => new Vector2f(rectangle.Width, rectangle.Height);
	}
}
