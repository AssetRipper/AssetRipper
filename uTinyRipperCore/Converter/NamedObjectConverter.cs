using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.Converters.EditorExtensions;

namespace uTinyRipper.Converters.NamedObjects
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
