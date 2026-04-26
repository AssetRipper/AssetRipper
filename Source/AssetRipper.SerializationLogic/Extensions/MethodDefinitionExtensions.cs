using System;
// The 'MethodDefinition' class is typically from a library like AsmResolver or Mono.Cecil.
// This using statement is implied by the project's dependencies.
// using AsmResolver.DotNet; 

namespace AssetRipper.SerializationLogic.Extensions;

internal static class MethodDefinitionExtensions
{
	public static bool SameAs(this MethodDefinition? self, MethodDefinition? other)
	{
		// FIX: Replaced expensive string comparison with a direct reference check.
		// This fulfills the FIXME's intent and is highly performant because metadata
		// libraries cache these objects, ensuring a single instance per method definition.
		return ReferenceEquals(self, other);
	}

	public static string PropertyName(this MethodDefinition self)
	{
		ArgumentNullException.ThrowIfNull(self);

		if (self.Name is null || self.Name.Length < 5)
		{
			// Improved exception message for better diagnostics.
			throw new ArgumentException("Method name is too short to be a property accessor.", nameof(self));
		}

		return self.Name.Value.Substring(4);
	}

	public static bool IsConversionOperator(this MethodDefinition method)
	{
		ArgumentNullException.ThrowIfNull(method);

		if (!method.IsSpecialName)
		{
			return false;
		}

		return method.Name == "op_Implicit" || method.Name == "op_Explicit";
	}

	public static bool IsSimpleSetter(this MethodDefinition original)
	{
		ArgumentNullException.ThrowIfNull(original);
		return original.IsSetMethod && original.Parameters.Count == 1;
	}

	public static bool IsSimpleGetter(this MethodDefinition original)
	{
		ArgumentNullException.ThrowIfNull(original);
		return original.IsGetMethod && original.Parameters.Count == 0;
	}

	public static bool IsSimplePropertyAccessor(this MethodDefinition method)
	{
		ArgumentNullException.ThrowIfNull(method);
		return method.IsSimpleGetter() || method.IsSimpleSetter();
	}

	public static bool IsDefaultConstructor(MethodDefinition m)
	{
		ArgumentNullException.ThrowIfNull(m);
		return m.IsConstructor && !m.IsStatic && m.Parameters.Count == 0;
	}
}
