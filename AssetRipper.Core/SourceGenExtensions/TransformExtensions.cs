using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO;
using AssetRipper.Core.Math.Transformations;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Transform_;
using System.Collections.Generic;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class TransformExtensions
	{
		private const char PathSeparator = '/';

		public static Transformation ToTransformation(this ITransform transform)
		{
			return Transformation.Create(transform.LocalPosition_C4.CastToStruct(), transform.LocalRotation_C4.CastToStruct(), transform.LocalScale_C4.CastToStruct());
		}

		public static Transformation ToInverseTransformation(this ITransform transform)
		{
			return Transformation.CreateInverse(transform.LocalPosition_C4.CastToStruct(), transform.LocalRotation_C4.CastToStruct(), transform.LocalScale_C4.CastToStruct());
		}

		public static string GetRootPath(this ITransform transform)
		{
			string pre = string.Empty;
			if (!transform.Father_C4.IsNull())
			{
				pre = transform.Father_C4.GetAsset(transform.SerializedFile).GetRootPath() + PathSeparator;
			}
			return pre + transform.GetGameObject().NameString;
		}

		/// <summary>
		/// Find the sibling index (aka the root order) of the transform
		/// </summary>
		/// <param name="transform">The relevant transform</param>
		/// <returns>The sibling index of the transform</returns>
		/// <exception cref="Exception">if the transform cannot be found among the father's children</exception>
		public static int GetSiblingIndex(this ITransform transform)
		{
			if (transform.Father_C4.IsNull())
			{
				return 0;
			}
			ITransform father = transform.Father_C4.GetAsset(transform.SerializedFile);
			for (int i = 0; i < father.Children_C4.Count; i++)
			{
				IPPtr_Transform_ child = father.Children_C4[i];
				if (child.PathIndex == transform.PathID)
				{
					return i;
				}
			}
			throw new Exception("Transform hasn't been found among father's children");
		}

		public static ITransform? FindChild(this ITransform transform, string path)
		{
			if (path.Length == 0)
			{
				return transform;
			}
			return FindChild(transform, path, 0);
		}

		private static ITransform? FindChild(this ITransform transform, string path, int startIndex)
		{
			int separatorIndex = path.IndexOf(PathSeparator, startIndex);
			string childName = separatorIndex == -1 ?
				path.Substring(startIndex, path.Length - startIndex) :
				path.Substring(startIndex, separatorIndex - startIndex);
			foreach (ITransform child in transform.GetChildren())
			{
				IGameObject childGO = child.GetGameObject();
				if (childGO.NameString == childName)
				{
					return separatorIndex == -1 ? child : child.FindChild(path, separatorIndex + 1);
				}
			}
			return default;
		}

		public static IEnumerable<ITransform> GetChildren(this ITransform transform)
		{
			foreach (IPPtr_Transform_ childPtr in transform.Children_C4)
			{
				yield return childPtr.GetAsset(transform.SerializedFile);
			}
		}

		public static void ConvertToEditorFormat(this ITransform transform)
		{
			if (transform.Has_RootOrder_C4())
			{
				transform.RootOrder_C4 = transform.CalculateRootOrder();
			}
			if (transform.Has_LocalEulerAnglesHint_C4())
			{
				Vector3f eulerHints = new Quaternionf(
					transform.LocalRotation_C4.X,
					transform.LocalRotation_C4.Y,
					transform.LocalRotation_C4.Z,
					transform.LocalRotation_C4.W).ToEulerAngle();
				transform.LocalEulerAnglesHint_C4.SetValues(eulerHints.X, eulerHints.Y, eulerHints.Z);
			}
		}

		private static int CalculateRootOrder(this ITransform transform)
		{
			if (transform.Father_C4.IsNull())
			{
				return 0;
			}
			ITransform father = transform.Father_C4.GetAsset(transform.SerializedFile);
			AccessListBase<IPPtr_Transform_> children = father.Children_C4;
			for (int i = 0; i < children.Count; i++)
			{
				IPPtr_Transform_ child = children[i];
				if (child.PathIndex == transform.PathID)
				{
					return i;
				}
			}
			throw new Exception("Transform hasn't been found among father's children");
		}
	}
}
