using AssetRipper.Classes;

namespace AssetRipper.Converters
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
