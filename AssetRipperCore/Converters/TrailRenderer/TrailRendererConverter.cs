using AssetRipper.Core.Project;
using AssetRipper.Core.Classes.TrailRenderer;

namespace AssetRipper.Core.Converters.TrailRenderer
{
	public static class TrailRendererConverter
	{
		public static AssetRipper.Core.Classes.TrailRenderer.TrailRenderer Convert(IExportContainer container, AssetRipper.Core.Classes.TrailRenderer.TrailRenderer origin)
		{
			AssetRipper.Core.Classes.TrailRenderer.TrailRenderer instance = new AssetRipper.Core.Classes.TrailRenderer.TrailRenderer(origin.AssetInfo);
			RendererConverter.Convert(container, origin, instance);
			instance.Time = origin.Time;
			instance.Parameters = GetParameters(container, origin);
			instance.MinVertexDistance = origin.MinVertexDistance;
			instance.Autodestruct = origin.Autodestruct;
			if (AssetRipper.Core.Classes.TrailRenderer.TrailRenderer.HasEmitting(container.ExportVersion))
			{
				instance.Emitting = GetEmitting(container, origin.Emitting);
			}
			return instance;
		}

		private static LineParameters GetParameters(IExportContainer container, AssetRipper.Core.Classes.TrailRenderer.TrailRenderer origin)
		{
			if (AssetRipper.Core.Classes.TrailRenderer.TrailRenderer.HasParameters(container.Version))
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
			return AssetRipper.Core.Classes.TrailRenderer.TrailRenderer.HasEmitting(container.Version) ? origin : true;
		}
	}
}
