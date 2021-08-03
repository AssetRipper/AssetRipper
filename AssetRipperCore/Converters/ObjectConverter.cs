using AssetRipper.Core.Project;
using AssetRipper.Core.Layout.Classes;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Converters
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
				AssetRipper.Core.Classes.GameObject.GameObject originGameObject = (AssetRipper.Core.Classes.GameObject.GameObject)origin;
				int depth = originGameObject.GetRootDepth();
				return depth > 1 ? HideFlags.HideInHierarchy : HideFlags.None;
			}
			return container.ExportFlags.IsForPrefab() ? HideFlags.HideInHierarchy : HideFlags.None;
		}
	}
}
