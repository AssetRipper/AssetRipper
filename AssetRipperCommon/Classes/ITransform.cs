using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Math.Vectors;
using System;

namespace AssetRipper.Core.Classes
{
	public interface ITransform : IComponent
	{
		PPtr<ITransform> FatherPtr { get; }
		PPtr<ITransform>[] ChildrenPtrs { get; }
		Vector3f LocalPosition { get; set; }
		Quaternionf LocalRotation { get; set; }
		Vector3f LocalScale { get; set; }
	}

	public static class TransformExtensions
	{
		private const char PathSeparator = '/';

		public static string GetRootPath(this ITransform transform)
		{
			string pre = string.Empty;
			if (!transform.FatherPtr.IsNull)
			{
				pre = transform.FatherPtr.GetAsset(transform.SerializedFile).GetRootPath() + PathSeparator;
			}
			return pre + transform.GetGameObject().Name;
		}

		/// <summary>
		/// Find the sibling index (aka the root order) of the transform
		/// </summary>
		/// <param name="transform">The relevant transform</param>
		/// <returns>The sibling index of the transform</returns>
		/// <exception cref="Exception">if the transform cannot be found among the father's children</exception>
		public static int GetSiblingIndex(this ITransform transform)
		{
			if (transform.FatherPtr.IsNull)
			{
				return 0;
			}
			ITransform father = transform.FatherPtr.GetAsset(transform.SerializedFile);
			PPtr<ITransform>[] children = father.ChildrenPtrs;
			for (int i = 0; i < children.Length; i++)
			{
				PPtr<ITransform> child = children[i];
				if (child.PathID == transform.PathID)
				{
					return i;
				}
			}
			throw new Exception("Transform hasn't been found among father's children");
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
				ITransform child = childPtr.GetAsset(transform.SerializedFile);
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
