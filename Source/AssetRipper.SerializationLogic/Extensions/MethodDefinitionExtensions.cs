using System;

// Note: AsmResolver namespaces are implied by the project.
// using AsmResolver.DotNet;

namespace AssetRipper.SerializationLogic.Extensions;

internal static class MethodDefinitionExtensions
{
	/// <summary>
	/// Compares two method definitions for reference equality.
	/// </summary>
	/// <remarks>
	/// This check avoids expensive string comparisons. It relies on the metadata 
	/// reader (e.g., AsmResolver) interning MethodDefinition instances, meaning 
	/// the same logical method within a single module will resolve to the same object reference.
	/// </remarks>
	public static bool SameAs(this MethodDefinition? self, MethodDefinition? other)
	{
		return ReferenceEquals(self, other);
	}

	/// <summary>
	/// Extracts the underlying property name from a property accessor method (e.g., "get_MyProp" -> "MyProp").
	/// </summary>
	public static string PropertyName(this MethodDefinition self)
	{
		ArgumentNullException.ThrowIfNull(self);

		// self.Name is a Utf8String. self.Name.Length is the byte count. 
		// We must check the decoded string length (self.Name.Value.Length) to safely slice characters.
		if (self.Name?.Value is not { Length: >= 5 } nameValue || 
		    !(nameValue.StartsWith("get_") || nameValue.StartsWith("set_")))
		{
			throw new ArgumentException($"Method name '{self.Name?.Value}' is not a valid property accessor.", nameof(self));
		}

		return nameValue[4..];
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

	public static bool IsDefaultConstructor(this MethodDefinition m)
	{
		ArgumentNullException.ThrowIfNull(m);
		return m.IsConstructor && !m.IsStatic && m.Parameters.Count == 0;
	}
}
