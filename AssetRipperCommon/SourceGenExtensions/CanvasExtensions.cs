using AssetRipper.Core.Classes.UI.Canvas;
using AssetRipper.SourceGenerated.Classes.ClassID_223;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class CanvasExtensions
	{
		public static RenderMode GetRenderMode(this ICanvas canvas)
		{
			return (RenderMode)canvas.RenderMode_C223;
		}
	}
}
