using AssetRipper.Core.Classes;
using AssetRipper.Core.Project;
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
			instance.m_LocalRotation = origin.m_LocalRotation;
			instance.m_LocalPosition = origin.m_LocalPosition;
			instance.m_LocalScale = origin.m_LocalScale;
			instance.Children = origin.Children.ToArray();
			instance.Father = origin.Father;
		}
	}
}
