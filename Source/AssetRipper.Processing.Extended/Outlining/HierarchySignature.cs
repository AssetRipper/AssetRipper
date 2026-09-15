using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Extensions;
using System.Text;

namespace AssetRipper.Processing.Extended.Outlining;

/// <summary>
/// Structural identity of a GameObject subtree: names, component class ids, and child shape.
/// Deliberately excludes component field values, so two hierarchies differing only in, say,
/// a transform position still share a signature.
/// </summary>
public static class HierarchySignature
{
	public static string Compute(IGameObject root)
	{
		StringBuilder builder = new();
		Append(builder, root);
		return builder.ToString();
	}

	private static void Append(StringBuilder builder, IGameObject gameObject)
	{
		builder.Append(gameObject.Name).Append('[');

		bool first = true;
		foreach (IComponent component in gameObject.GetComponentAccessList())
		{
			if (!first)
			{
				builder.Append(',');
			}
			builder.Append(component.ClassID);
			first = false;
		}

		builder.Append("](");
		foreach (IGameObject child in gameObject.GetChildren())
		{
			Append(builder, child);
		}
		builder.Append(')');
	}

	/// <summary>
	/// Walks two subtrees in lockstep and records which asset of <paramref name="duplicate"/>
	/// corresponds to which asset of <paramref name="canonical"/>.
	/// </summary>
	/// <remarks>
	/// Returns false at the first sign of divergence and leaves <paramref name="pairs"/> to be
	/// discarded by the caller. A wrong pairing would silently point references at the wrong
	/// component, so equal signatures alone are not trusted — the structure is re-verified here.
	/// </remarks>
	public static bool TryPair(IGameObject duplicate, IGameObject canonical, List<(IUnityObjectBase Duplicate, IUnityObjectBase Canonical)> pairs)
	{
		if (duplicate.Name != canonical.Name)
		{
			return false;
		}

		pairs.Add((duplicate, canonical));

		List<IComponent> duplicateComponents = [.. duplicate.GetComponentAccessList()];
		List<IComponent> canonicalComponents = [.. canonical.GetComponentAccessList()];
		if (duplicateComponents.Count != canonicalComponents.Count)
		{
			return false;
		}

		for (int i = 0; i < duplicateComponents.Count; i++)
		{
			if (duplicateComponents[i].ClassID != canonicalComponents[i].ClassID)
			{
				return false;
			}
			pairs.Add((duplicateComponents[i], canonicalComponents[i]));
		}

		List<IGameObject> duplicateChildren = [.. duplicate.GetChildren()];
		List<IGameObject> canonicalChildren = [.. canonical.GetChildren()];
		if (duplicateChildren.Count != canonicalChildren.Count)
		{
			return false;
		}

		for (int i = 0; i < duplicateChildren.Count; i++)
		{
			if (!TryPair(duplicateChildren[i], canonicalChildren[i], pairs))
			{
				return false;
			}
		}

		return true;
	}
}
