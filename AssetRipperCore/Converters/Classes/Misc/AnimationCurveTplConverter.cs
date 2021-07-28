using AssetRipper.Converters.Project;
using AssetRipper.Layout.Classes.Misc.Serializable;
using AssetRipper.Parser.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.IO.Asset;
using System.Linq;

namespace AssetRipper.Converters.Classes.Misc
{
	public static class AnimationCurveTplConverter
	{
		public static AnimationCurveTpl<T> Convert<T>(IExportContainer container, ref AnimationCurveTpl<T> origin) where T : struct, IAsset
		{
			AnimationCurveTplLayout exlayout = container.ExportLayout.Serialized.AnimationCurveTpl;
			AnimationCurveTpl<T> instance = new AnimationCurveTpl<T>();
			instance.Curve = origin.Curve.Select(t => t.Convert(container)).ToArray();
			instance.PreInfinity = origin.PreInfinity;
			instance.PostInfinity = origin.PostInfinity;
			if (exlayout.HasRotationOrder)
			{
				instance.RotationOrder = GetRotationOrder(container, ref origin);
			}
			return instance;
		}

		private static RotationOrder GetRotationOrder<T>(IExportContainer container, ref AnimationCurveTpl<T> origin) where T : struct, IAsset
		{
			return container.Layout.Serialized.AnimationCurveTpl.HasRotationOrder ? origin.RotationOrder : RotationOrder.OrderZXY;
		}
	}
}
