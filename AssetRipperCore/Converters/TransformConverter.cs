using AssetRipper.Core.Project;
using AssetRipper.Core.Classes;
using System.Linq;

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
		}
	}
}
