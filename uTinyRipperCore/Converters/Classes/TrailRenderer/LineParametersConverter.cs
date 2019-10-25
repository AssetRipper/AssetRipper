using uTinyRipper.Classes.TrailRenderers;

namespace uTinyRipper.Converters.TrailRenderers
{
	public static class LineParametersConverter
	{
		public static LineParameters Convert(IExportContainer container, ref LineParameters origin)
		{
			LineParameters instance = origin;
			instance.WidthCurve = origin.WidthCurve.Convert(container);
			if (LineParameters.HasShadowBias(container.ExportVersion))
			{
				instance.ShadowBias = GetShadowBias(container, ref origin);
			}
			return instance;
		}

		private static float GetShadowBias(IExportContainer container, ref LineParameters origin)
		{
			return LineParameters.HasShadowBias(container.Version) ? origin.ShadowBias : 0.5f;
		}
	}
}