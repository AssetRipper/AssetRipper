using System.Linq;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Converters.Misc
{
	public static class AnimationCurveTplConverter
	{
		public static AnimationCurveTpl<T> Convert<T>(IExportContainer container, ref AnimationCurveTpl<T> origin)
			where T : struct, IAsset
		{
			AnimationCurveTpl<T> instance = new AnimationCurveTpl<T>();
			instance.Curve = origin.Curve.Select(t => t.Convert(container)).ToArray();
			instance.PreInfinity = origin.PreInfinity;
			instance.PostInfinity = origin.PostInfinity;
			if (AnimationCurveTpl<T>.HasRotationOrder(container.ExportVersion))
			{
				instance.RotationOrder = GetRotationOrder(container, ref origin);
			}
			return instance;
		}

		private static RotationOrder GetRotationOrder<T>(IExportContainer container, ref AnimationCurveTpl<T> origin)
			where T : struct, IAsset
		{
			return AnimationCurveTpl<T>.HasRotationOrder(container.Version) ? origin.RotationOrder : RotationOrder.OrderZXY;
		}
	}
}
