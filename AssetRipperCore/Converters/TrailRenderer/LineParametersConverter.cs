﻿using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.Classes.TrailRenderer;
using AssetRipper.Core.Converters.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters.TrailRenderer
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

		private static AnimationCurveTpl<T> Convert<T>(this AnimationCurveTpl<T> _this, IExportContainer container) where T : struct, IAsset, IYAMLExportable
		{
			return AnimationCurveTplConverter.Convert(container, ref _this);
		}
	}
}