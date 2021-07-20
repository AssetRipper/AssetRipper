using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.TrailRenderer;

namespace AssetRipper.Converters.Classes.TrailRenderer
{
	public static class TrailRendererConverter
	{
		public static Parser.Classes.TrailRenderer.TrailRenderer Convert(IExportContainer container, Parser.Classes.TrailRenderer.TrailRenderer origin)
		{
			Parser.Classes.TrailRenderer.TrailRenderer instance = new Parser.Classes.TrailRenderer.TrailRenderer(origin.AssetInfo);
			RendererConverter.Convert(container, origin, instance);
			instance.Time = origin.Time;
			instance.Parameters = GetParameters(container, origin);
			instance.MinVertexDistance = origin.MinVertexDistance;
			instance.Autodestruct = origin.Autodestruct;
			if (Parser.Classes.TrailRenderer.TrailRenderer.HasEmitting(container.ExportVersion))
			{
				instance.Emitting = GetEmitting(container, origin.Emitting);
			}
			return instance;
		}

		private static LineParameters GetParameters(IExportContainer container, Parser.Classes.TrailRenderer.TrailRenderer origin)
		{
			if (Parser.Classes.TrailRenderer.TrailRenderer.HasParameters(container.Version))
			{
				return origin.Parameters.Convert(container);
			}
			else
			{
				LineParameters instance = new LineParameters(container.ExportVersion);
				instance.WidthCurve = origin.Parameters.WidthCurve.Convert(container);
				instance.ColorGradient = origin.Colors.GenerateGragient(container);
				return instance;
			}
		}

		private static bool GetEmitting(IExportContainer container, bool origin)
		{
			return Parser.Classes.TrailRenderer.TrailRenderer.HasEmitting(container.Version) ? origin : true;
		}
	}
}
