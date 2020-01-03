using uTinyRipper.Classes;
using uTinyRipper.Classes.TrailRenderers;

namespace uTinyRipper.Converters
{
	public static class TrailRendererConverter
	{
		public static TrailRenderer Convert(IExportContainer container, TrailRenderer origin)
		{
			TrailRenderer instance = new TrailRenderer(origin.AssetInfo);
			RendererConverter.Convert(container, origin, instance);
			instance.Time = origin.Time;
			instance.Parameters = GetParameters(container, origin);
			instance.MinVertexDistance = origin.MinVertexDistance;
			instance.Autodestruct = origin.Autodestruct;
			if (TrailRenderer.HasEmitting(container.ExportVersion))
			{
				instance.Emitting = GetEmitting(container, origin.Emitting);
			}
			return instance;
		}

		private static LineParameters GetParameters(IExportContainer container, TrailRenderer origin)
		{
			if (TrailRenderer.HasParameters(container.Version))
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
			return TrailRenderer.HasEmitting(container.Version) ? origin : true;
		}
	}
}
