using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes;

namespace AssetRipper.Converters.Classes
{
	public static class RectTransformConverter
	{
		public static RectTransform Convert(IExportContainer container, RectTransform origin)
		{
			RectTransform instance = new RectTransform(container.ExportLayout);
			TransformConverter.Convert(container, origin, instance);
			instance.AnchorMin = origin.AnchorMin;
			instance.AnchorMax = origin.AnchorMax;
			instance.AnchorPosition = origin.AnchorPosition;
			instance.SizeDelta = origin.SizeDelta;
			instance.Pivot = origin.Pivot;
			return instance;
		}
	}
}
