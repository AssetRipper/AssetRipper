using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes;

namespace AssetRipper.Converters.Classes
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
