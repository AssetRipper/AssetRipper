using AssetRipper.SourceGenerated.Subclasses.Rectf;
using System.Numerics;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class RectfExtensions
	{
		public static Vector2 Center(this IRectf rectangle) => new Vector2(rectangle.X + (rectangle.Width / 2.0f), rectangle.Y + (rectangle.Height / 2.0f));
		public static Vector2 Position(this IRectf rectangle) => new Vector2(rectangle.X, rectangle.Y);
		public static Vector2 Size(this IRectf rectangle) => new Vector2(rectangle.Width, rectangle.Height);
	}
}
