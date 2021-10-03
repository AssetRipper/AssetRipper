using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters
{
	public static class ObjectConverter
	{
		public static void Convert(IExportContainer container, Object origin, Object instance)
		{
			instance.AssetInfo = origin.AssetInfo;
			if (Object.HasHideFlag(container.ExportVersion, container.ExportFlags))
			{
				instance.ObjectHideFlags = GetObjectHideFlags(container, origin);
			}
		}

		private static HideFlags GetObjectHideFlags(IExportContainer container, Object origin)
		{
			if (Object.HasHideFlag(container.Version, container.Flags))
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
