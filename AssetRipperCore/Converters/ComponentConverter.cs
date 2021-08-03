using AssetRipper.Core.Project;
using AssetRipper.Core.Classes;

namespace AssetRipper.Core.Converters
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
