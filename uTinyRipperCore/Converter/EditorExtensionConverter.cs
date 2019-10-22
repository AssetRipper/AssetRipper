using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;

namespace uTinyRipper.Converters
{
	public static class EditorExtensionConverter
	{
		public static void Convert(IExportContainer container, EditorExtension origin, EditorExtension instance)
		{
			ObjectConverter.Convert(container, origin, instance);
#if UNIVERSAL
			if (EditorExtension.HasCorrespondingSourceObject(container.ExportFlags))
			{
				instance.CorrespondingSourceObject = GetCorrespondingSourceObject(container, origin);
				instance.PrefabInstance = GetPrefabInstance(container, origin);
			}
			if (EditorExtension.HasPrefabAsset(container.ExportVersion, container.ExportFlags))
			{
				instance.PrefabAsset = GetPrefabAsset(container, origin);
			}
#endif
		}

#if UNIVERSAL
		private static PPtr<EditorExtension> GetCorrespondingSourceObject(IExportContainer container, EditorExtension origin)
		{
			if (EditorExtension.HasCorrespondingSourceObject(container.Flags))
			{
				return origin.CorrespondingSourceObject;
			}
			return default;
		}

		private static PPtr<Prefab> GetPrefabInstance(IExportContainer container, EditorExtension origin)
		{
			if (EditorExtension.HasCorrespondingSourceObject(container.Flags))
			{
				return origin.PrefabInstance;
			}
			return default;
		}

		private static PPtr<Object> GetPrefabAsset(IExportContainer container, EditorExtension origin)
		{
			if (EditorExtension.HasPrefabAsset(container.Version, container.Flags))
			{
				return origin.PrefabAsset;
			}
			return default;
		}
#endif
	}
}
