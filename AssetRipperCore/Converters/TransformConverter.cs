using AssetRipper.Core.Project;
using AssetRipper.Core.Classes;
using System.Linq;
#if UNIVERSAL
using AssetRipper.Core.Math;
#endif

namespace AssetRipper.Core.Converters
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
			if (Transform.HasRootOrder(container.ExportVersion, container.ExportFlags))
			{
				instance.RootOrder = GetRootOrder(container, origin);
			}
			if (Transform.HasLocalEulerAnglesHint(container.ExportVersion, container.ExportFlags))
			{
				instance.LocalEulerAnglesHint = GetLocalEulerAnglesHint(container, origin);
			}
#endif
		}

#if UNIVERSAL
		private static int GetRootOrder(IExportContainer container, Transform origin)
		{
			if (Transform.HasRootOrder(container.Version, container.Flags))
			{
				return origin.RootOrder;
			}
			return origin.GetSiblingIndex();
		}

		private static Vector3f GetLocalEulerAnglesHint(IExportContainer container, Transform origin)
		{
			if (Transform.HasLocalEulerAnglesHint(container.Version, container.Flags))
			{
				return origin.LocalEulerAnglesHint;
			}
			return origin.LocalRotation.ToEuler();
		}
#endif
	}
}
