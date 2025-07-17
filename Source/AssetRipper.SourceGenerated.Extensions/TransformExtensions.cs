using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_4;

namespace AssetRipper.SourceGenerated.Extensions;

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
		string name = transform.GameObject_C4P?.Name ?? "Unnamed";
		if (transform.Father_C4P is { } father)
		{
			return $"{father.GetRootPath()}{PathSeparator}{name}";
		}
		else
		{
			return name;
		}
	}

	/// <summary>
	/// Initialize an injected Transform with some sensible default values.
	/// </summary>
	/// <remarks>
	/// Since this Transform is assumed to have no <see cref="ITransform.Father_C4"/>, its <see cref="ITransform.RootOrder_C4"/> is zero.
	/// </remarks>
	/// <param name="transform"></param>
	public static void InitializeDefault(this ITransform transform)
	{
		transform.LocalPosition_C4.SetZero();
		transform.LocalRotation_C4.SetIdentity();
		transform.LocalScale_C4.SetOne();
		transform.RootOrder_C4 = 0;
		transform.LocalEulerAnglesHint_C4?.SetZero();
	}

	public static ITransform? FindChild(this ITransform transform, string path)
	{
		if (path.Length == 0)
		{
			return transform;
		}
		return transform.FindChild(path, 0);
	}

	private static ITransform? FindChild(this ITransform transform, string path, int startIndex)
	{
		int separatorIndex = path.IndexOf(PathSeparator, startIndex);
		string childName = separatorIndex == -1 ? path[startIndex..] : path[startIndex..separatorIndex];
		foreach (ITransform child in transform.Children_C4P.WhereNotNull())
		{
			IGameObject? childGO = child.GameObject_C4P;
			if (childGO is null)
			{
				continue;
			}
			if (childGO.Name == childName)
			{
				return separatorIndex == -1 ? child : child.FindChild(path, separatorIndex + 1);
			}
		}
		return default;
	}

	/// <summary>
	/// Find the sibling index (aka the root order) of the transform
	/// </summary>
	/// <param name="transform">The relevant transform</param>
	/// <returns>The sibling index of the transform</returns>
	/// <exception cref="Exception">if the transform cannot be found among the father's children</exception>
	public static int CalculateRootOrder(this ITransform transform)
	{
		ITransform? father = transform.Father_C4P;
		if (father is null)
		{
			return 0;
		}
		for (int i = father.Children_C4.Count - 1; i >= 0; i--)
		{
			// Performance optimization: check PathID first to avoid unnecessary asset resolution.
			if (father.Children_C4[i].PathID != transform.PathID)
			{
				continue;
			}

			if (father.Children_C4P[i] == transform)
			{
				return i;
			}
		}
		throw new Exception("Transform hasn't been found among father's children");
	}
}
