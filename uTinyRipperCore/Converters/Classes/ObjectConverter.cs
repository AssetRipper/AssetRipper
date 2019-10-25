using uTinyRipper.Classes;
using uTinyRipper.Classes.Objects;

namespace uTinyRipper.Converters
{
	public static class ObjectConverter
	{
		public static void Convert(IExportContainer container, Object origin, Object instance)
		{
			if (Object.HasHideFlag(container.ExportVersion, container.ExportFlags))
			{
				instance.ObjectHideFlags = GetObjectHideFlags(container, origin);
			}
#if UNIVERSAL
			if (Object.HasInstanceID(container.ExportVersion, container.ExportFlags))
			{
				instance.InstanceID = origin.InstanceID;
				instance.LocalIdentfierInFile = origin.LocalIdentfierInFile;
			}
#endif
		}

		private static HideFlags GetObjectHideFlags(IExportContainer container, Object origin)
		{
			if (Object.HasHideFlag(container.Version, container.Flags))
			{
				return origin.ObjectHideFlags;
			}
			if (origin.ClassID == ClassIDType.GameObject)
			{
				GameObject originGameObject = (GameObject)origin;
				int depth = originGameObject.GetRootDepth();
				return depth > 1 ? HideFlags.HideInHierarchy : HideFlags.None;
			}
			return container.ExportFlags.IsForPrefab() ? HideFlags.HideInHierarchy : HideFlags.None;
		}
	}
}
