using AssetRipper.Classes;
using AssetRipper.Classes.Objects;
using AssetRipper.Layout;

namespace AssetRipper.Converters
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
