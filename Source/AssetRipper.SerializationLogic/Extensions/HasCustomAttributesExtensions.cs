using AsmResolver;

namespace AssetRipper.SerializationLogic.Extensions;

internal static class HasCustomAttributesExtensions
{
	public static bool HasSerializeFieldAttribute(this IHasCustomAttribute owner)
	{
		return owner.HasCustomAttribute(EngineTypePredicates.UnityEngineNamespace, EngineTypePredicates.SerializeFieldAttribute);
	}

	public static bool HasSerializeReferenceAttribute(this IHasCustomAttribute owner)
	{
		return owner.HasCustomAttribute(EngineTypePredicates.UnityEngineNamespace, EngineTypePredicates.SerializeReferenceAttribute);
	}

	public static bool HasFixedBufferAttribute(this IHasCustomAttribute owner)
	{
		return owner.HasCustomAttribute("System.Runtime.CompilerServices", "FixedBufferAttribute");
	}

	public static CustomAttribute? GetCustomAttribute(this IHasCustomAttribute owner, string? ns, string? name)
	{
		for (int i = 0; i < owner.CustomAttributes.Count; i++)
		{
			CustomAttribute attribute = owner.CustomAttributes[i];
			ITypeDefOrRef? declaringType = attribute.Type;
			if (declaringType == null)
			{
				continue;
			}
			if (declaringType.Namespace == ns && declaringType.Name == name)
			{
				return attribute;
			}
		}
		return null;
	}

	public static CustomAttribute? GetCustomAttribute(this IHasCustomAttribute owner, Utf8String? ns, Utf8String? name)
	{
		for (int i = 0; i < owner.CustomAttributes.Count; i++)
		{
			CustomAttribute attribute = owner.CustomAttributes[i];
			ITypeDefOrRef? declaringType = attribute.Type;
			if (declaringType == null)
			{
				continue;
			}
			if (declaringType.Namespace == ns && declaringType.Name == name)
			{
				return attribute;
			}
		}
		return null;
	}

	public static int GetFixedBufferSize(this IHasCustomAttribute owner)
	{
		CustomAttribute? attribute = owner.GetCustomAttribute("System.Runtime.CompilerServices", "FixedBufferAttribute")
			?? throw new InvalidOperationException("The owner does not have a FixedBufferAttribute.");

		int size = (int)(attribute.Signature?.FixedArguments[1].Element ?? 0);

		return size;
	}

	public static TypeSignature GetFixedBufferElementType(this IHasCustomAttribute owner)
	{
		CustomAttribute? attribute = owner.GetCustomAttribute("System.Runtime.CompilerServices", "FixedBufferAttribute")
			?? throw new InvalidOperationException("The owner does not have a FixedBufferAttribute.");
		object elementType = attribute.Signature?.FixedArguments[0].Element
			?? throw new InvalidOperationException("The FixedBufferAttribute does not have a valid element type.");
		return (TypeSignature)elementType;
	}
}
