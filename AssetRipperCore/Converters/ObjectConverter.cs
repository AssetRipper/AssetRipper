using AssetRipper.Project;
using AssetRipper.Layout.Classes;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Object;
using AssetRipper.IO.Asset;

namespace AssetRipper.Converters
{
	public static class ObjectConverter
	{
		public static void Convert(IExportContainer container, UnityObject origin, UnityObject instance)
		{
			instance.AssetInfo = origin.AssetInfo;
			ObjectLayout exlayout = container.ExportLayout.Object;
			if (exlayout.HasHideFlag)
			{
				instance.ObjectHideFlags = GetObjectHideFlags(container, origin);
			}
		}

		private static HideFlags GetObjectHideFlags(IExportContainer container, UnityObject origin)
		{
			if (container.Layout.Object.HasHideFlag)
			{
				return origin.ObjectHideFlags;
			}
#warning TODO: set those flags at the moment of creation a prefab
			if (origin.ClassID == ClassIDType.GameObject)
			{
				AssetRipper.Classes.GameObject.GameObject originGameObject = (AssetRipper.Classes.GameObject.GameObject)origin;
				int depth = originGameObject.GetRootDepth();
				return depth > 1 ? HideFlags.HideInHierarchy : HideFlags.None;
			}
			return container.ExportFlags.IsForPrefab() ? HideFlags.HideInHierarchy : HideFlags.None;
		}
	}
}
