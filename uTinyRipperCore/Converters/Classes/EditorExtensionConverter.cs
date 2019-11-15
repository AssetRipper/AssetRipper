using uTinyRipper.Classes;
using uTinyRipper.Layout;

namespace uTinyRipper.Converters
{
	public static class EditorExtensionConverter
	{
		public static void Convert(IExportContainer container, EditorExtension origin, EditorExtension instance)
		{
			EditorExtensionLayout layout = container.Layout.EditorExtension;
			EditorExtensionLayout exlayout = container.ExportLayout.EditorExtension;
			ObjectConverter.Convert(container, origin, instance);
#if UNIVERSAL
			if (exlayout.HasCorrespondingSourceObjectInvariant)
			{
				if (layout.HasCorrespondingSourceObjectInvariant)
				{
					instance.CorrespondingSourceObject = origin.CorrespondingSourceObject;
					instance.PrefabInstance = origin.PrefabInstance;
				}
				else
				{
#warning TODO: get values from ExtensionPtr
				}
			}
			else
			{
				instance.ExtensionPtr = origin.ExtensionPtr;
			}
			if (exlayout.HasPrefabAsset && layout.HasPrefabAsset)
			{
				instance.PrefabAsset = origin.PrefabAsset;
			}
#endif
		}
	}
}
