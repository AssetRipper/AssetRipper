using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using System.Linq;

namespace AssetRipper.Core.Converters.Misc
{
	public static class AnimationCurveTplConverter
	{
		public static AnimationCurveTpl<T> Convert<T>(IExportContainer container, AnimationCurveTpl<T> origin) where T : IAsset, new()
		{
			AnimationCurveTpl<T> instance = new AnimationCurveTpl<T>();
			instance.Curve = origin.Curve.Select(t => t.ConvertKeyframe(container)).ToArray();
			instance.PreInfinity = origin.PreInfinity;
			instance.PostInfinity = origin.PostInfinity;
			if (AnimationCurveTpl<T>.HasRotationOrder(container.ExportVersion))
			{
				instance.RotationOrder = GetRotationOrder(container, origin);
			}
			return instance;
		}

		private static RotationOrder GetRotationOrder<T>(IExportContainer container, AnimationCurveTpl<T> origin) where T : IAsset, new()
		{
			return AnimationCurveTpl<T>.HasRotationOrder(container.Version) ? origin.RotationOrder : RotationOrder.OrderZXY;
		}

		private static KeyframeTpl<T> ConvertKeyframe<T>(this KeyframeTpl<T> _this, IExportContainer container) where T : IAsset, new()
		{
			return KeyframeTplConverter.Convert(container, _this);
		}
	}
}
