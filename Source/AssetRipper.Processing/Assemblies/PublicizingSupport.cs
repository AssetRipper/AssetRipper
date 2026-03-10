using AsmResolver.DotNet;
using AssetRipper.CIL;

namespace AssetRipper.Processing.Assemblies;

internal static class PublicizingSupport
{
	public static bool ShouldSkipMethodPublicizing(MethodDefinition method)
	{
		if (method.IsStaticConstructor())
		{
			return true;
		}

		if (method.Name == "Finalize"
			&& method.IsVirtual
			&& method.IsFamily
			&& method.DeclaringType!.MethodImplementations.Any(implementation => implementation.Body == method))
		{
			return true;
		}

		if (method.DeclaringType!.MethodImplementations.Any(implementation => implementation.Body == method))
		{
			return true;
		}

		// Preserve the original accessibility of true overrides so decompiled code keeps valid base-member signatures.
		return method.IsVirtual && method.IsReuseSlot && !method.IsNewSlot;
	}

	public static void DeduplicateNonSerializedAttributes(FieldDefinition field)
	{
		bool keepFirst = false;
		for (int i = field.CustomAttributes.Count - 1; i >= 0; i--)
		{
			if (!field.CustomAttributes[i].IsNonSerializedAttribute())
			{
				continue;
			}

			if (!keepFirst)
			{
				keepFirst = true;
				continue;
			}

			field.CustomAttributes.RemoveAt(i);
		}
	}
}
