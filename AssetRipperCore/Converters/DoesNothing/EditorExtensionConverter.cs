using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters
{
	public static class EditorExtensionConverter
	{
		public static void Convert(IExportContainer container, EditorExtension origin, EditorExtension instance)
		{
			ObjectConverter.Convert(container, origin, instance);
			instance.PrefabInstance.CopyValues(origin.PrefabInstance);
		}
	}
}
