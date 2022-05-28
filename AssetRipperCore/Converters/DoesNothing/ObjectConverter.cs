using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters
{
	public static class ObjectConverter
	{
		public static void Convert(IExportContainer container, Classes.Object.Object origin, Classes.Object.Object instance)
		{
			instance.AssetInfo = origin.AssetInfo;
			instance.ObjectHideFlagsOld = origin.ObjectHideFlagsOld;
		}
	}
}
