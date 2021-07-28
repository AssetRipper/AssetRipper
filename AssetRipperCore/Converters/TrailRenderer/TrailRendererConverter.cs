using AssetRipper.Project;
using AssetRipper.Classes.TrailRenderer;

namespace AssetRipper.Converters.TrailRenderer
{
	public static class TrailRendererConverter
	{
		public static AssetRipper.Classes.TrailRenderer.TrailRenderer Convert(IExportContainer container, AssetRipper.Classes.TrailRenderer.TrailRenderer origin)
		{
			AssetRipper.Classes.TrailRenderer.TrailRenderer instance = new AssetRipper.Classes.TrailRenderer.TrailRenderer(origin.AssetInfo);
			RendererConverter.Convert(container, origin, instance);
			instance.Time = origin.Time;
			instance.Parameters = GetParameters(container, origin);
			instance.MinVertexDistance = origin.MinVertexDistance;
			instance.Autodestruct = origin.Autodestruct;
			if (AssetRipper.Classes.TrailRenderer.TrailRenderer.HasEmitting(container.ExportVersion))
			{
				instance.Emitting = GetEmitting(container, origin.Emitting);
			}
			return instance;
		}

		private static LineParameters GetParameters(IExportContainer container, AssetRipper.Classes.TrailRenderer.TrailRenderer origin)
		{
			if (AssetRipper.Classes.TrailRenderer.TrailRenderer.HasParameters(container.Version))
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
			return AssetRipper.Classes.TrailRenderer.TrailRenderer.HasEmitting(container.Version) ? origin : true;
		}
	}
}
