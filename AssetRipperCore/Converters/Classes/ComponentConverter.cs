using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes;

namespace AssetRipper.Converters.Classes
{
	public static class ComponentConverter
	{
		public static void Convert(IExportContainer container, Component origin, Component instance)
		{
			EditorExtensionConverter.Convert(container, origin, instance);
			instance.GameObject = origin.GameObject;
		}
	}
}
