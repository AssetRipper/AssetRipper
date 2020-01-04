using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.Layout;

namespace uTinyRipper.Converters
{
	public static class TransformConverter
	{
		public static Transform Convert(IExportContainer container, Transform origin)
		{
			Transform instance = new Transform(container.ExportLayout);
			Convert(container, origin, instance);
			return instance;
		}

		public static void Convert(IExportContainer container, Transform origin, Transform instance)
		{
			ComponentConverter.Convert(container, origin, instance);
			instance.LocalRotation = origin.LocalRotation;
			instance.LocalPosition = origin.LocalPosition;
			instance.LocalScale = origin.LocalScale;
			instance.Children = origin.Children.ToArray();
			instance.Father = origin.Father;
#if UNIVERSAL
			TransformLayout exlayout = container.ExportLayout.Transform;
			if (exlayout.HasRootOrder)
			{
				instance.RootOrder = GetRootOrder(container, origin);
			}
			if (exlayout.HasLocalEulerAnglesHint)
			{
				instance.LocalEulerAnglesHint = GetLocalEulerAnglesHint(container, origin);
			}
#endif
		}

#if UNIVERSAL
		private static int GetRootOrder(IExportContainer container, Transform origin)
		{
			if (container.Layout.Transform.HasRootOrder)
			{
				return origin.RootOrder;
			}
			return origin.GetSiblingIndex();
		}

		private static Vector3f GetLocalEulerAnglesHint(IExportContainer container, Transform origin)
		{
			if (container.Layout.Transform.HasLocalEulerAnglesHint)
			{
				return origin.LocalEulerAnglesHint;
			}
			return origin.LocalRotation.ToEuler();
		}
#endif
	}
}
