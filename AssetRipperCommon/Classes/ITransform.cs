using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using System;

namespace AssetRipper.Core.Classes
{
	public interface ITransform : IComponent
	{
		public PPtr<ITransform> FatherPtr { get; }
		public PPtr<ITransform>[] ChildrenPtrs { get; }
	}

	public static class TransformExtensions
	{
		private const char PathSeparator = '/';

		public static string GetRootPath(this ITransform transform)
		{
			string pre = string.Empty;
			if (!transform.FatherPtr.IsNull)
			{
				pre = transform.FatherPtr.GetAsset(transform.File).GetRootPath() + PathSeparator;
			}
			return pre + transform.GetGameObject().Name;
		}

		public static int GetSiblingIndex(this ITransform transform)
		{
			if (transform.FatherPtr.IsNull)
			{
				return 0;
			}
			ITransform father = transform.FatherPtr.GetAsset(transform.File);
			for (int i = 0; i < father.ChildrenPtrs.Length; i++)
			{
				PPtr<ITransform> child = father.ChildrenPtrs[i];
				if (child.PathID == transform.PathID)
				{
					return i;
				}
			}
			throw new Exception("Transorm hasn't been found among father's children");
		}

		public static ITransform FindChild(this ITransform transform, string path)
		{
			if (path.Length == 0)
			{
				return transform;
			}
			return FindChild(transform, path, 0);
		}

		private static ITransform FindChild(this ITransform transform, string path, int startIndex)
		{
			int separatorIndex = path.IndexOf(PathSeparator, startIndex);
			string childName = separatorIndex == -1 ?
				path.Substring(startIndex, path.Length - startIndex) :
				path.Substring(startIndex, separatorIndex - startIndex);
			foreach (PPtr<ITransform> childPtr in transform.ChildrenPtrs)
			{
				ITransform child = childPtr.GetAsset(transform.File);
				IGameObject childGO = child.GetGameObject();
				if (childGO.Name == childName)
				{
					return separatorIndex == -1 ? child : child.FindChild(path, separatorIndex + 1);
				}
			}
			return default;
		}
	}
}
