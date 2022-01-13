using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters
{
	public static class ObjectConverter
	{
		public static void Convert(IExportContainer container, Object origin, Object instance)
		{
			instance.AssetInfo = origin.AssetInfo;
			instance.ObjectHideFlags = origin.ObjectHideFlags;
		}
	}
}
