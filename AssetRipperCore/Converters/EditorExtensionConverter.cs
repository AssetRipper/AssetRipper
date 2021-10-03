using AssetRipper.Core.Classes;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters
{
	public static class EditorExtensionConverter
	{
		public static void Convert(IExportContainer container, EditorExtension origin, EditorExtension instance)
		{
			ObjectConverter.Convert(container, origin, instance);
#if UNIVERSAL
			if (EditorExtension.HasCorrespondingSourceObjectInvariant(container.ExportVersion, container.ExportFlags))
			{
				if (EditorExtension.HasCorrespondingSourceObjectInvariant(container.Version, container.Flags))
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
			if (EditorExtension.HasPrefabAsset(container.ExportVersion, container.ExportFlags) && EditorExtension.HasPrefabAsset(container.Version, container.Flags))
			{
				instance.PrefabAsset = origin.PrefabAsset;
			}
#endif
		}
	}
}
