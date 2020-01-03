using uTinyRipper.Classes;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Layout;

namespace uTinyRipper.Converters
{
	public static class ObjectConverter
	{
		public static void Convert(IExportContainer container, Object origin, Object instance)
		{
			instance.AssetInfo = origin.AssetInfo;
			ObjectLayout exlayout = container.ExportLayout.Object;
			if (exlayout.HasHideFlag)
			{
				instance.ObjectHideFlags = GetObjectHideFlags(container, origin);
			}
		}

		private static HideFlags GetObjectHideFlags(IExportContainer container, Object origin)
		{
			if (container.Layout.Object.HasHideFlag)
			{
				return origin.ObjectHideFlags;
			}
#warning TODO: set those flags at the moment of creation a prefab
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
