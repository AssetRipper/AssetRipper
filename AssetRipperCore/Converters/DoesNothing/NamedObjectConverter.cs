using AssetRipper.Core.Classes;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters
{
	public static class NamedObjectConverter
	{
		public static void Convert(IExportContainer container, NamedObject origin, NamedObject instance)
		{
			EditorExtensionConverter.Convert(container, origin, instance);
			instance.Name = origin.Name;
		}
	}
}
