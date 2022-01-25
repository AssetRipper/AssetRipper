﻿using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using System.Linq;

namespace AssetRipper.Core.Converters.Misc
{
	public static class AnimationCurveTplConverter
	{
		public static AnimationCurveTpl<T> Convert<T>(IExportContainer container, ref AnimationCurveTpl<T> origin) where T : struct, IAsset
		{
			AnimationCurveTpl<T> instance = new AnimationCurveTpl<T>();
			instance.Curve = origin.Curve.Select(t => t.ConvertKeyframe(container)).ToArray();
			instance.PreInfinity = origin.PreInfinity;
			instance.PostInfinity = origin.PostInfinity;
			if (AnimationCurveTpl<T>.HasRotationOrder(container.ExportVersion))
			{
				instance.RotationOrder = GetRotationOrder(container, ref origin);
			}
			return instance;
		}

		private static RotationOrder GetRotationOrder<T>(IExportContainer container, ref AnimationCurveTpl<T> origin) where T : struct, IAsset
		{
			return AnimationCurveTpl<T>.HasRotationOrder(container.Version) ? origin.RotationOrder : RotationOrder.OrderZXY;
		}

		private static KeyframeTpl<T> ConvertKeyframe<T>(this KeyframeTpl<T> _this, IExportContainer container) where T : struct, IAsset
		{
			return KeyframeTplConverter.Convert(container, ref _this);
		}
	}
}
