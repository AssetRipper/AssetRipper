namespace AssetRipper.SerializationLogic.Extensions;

static class MethodDefinitionExtensions
{
	public static bool SameAs(this MethodDefinition self, MethodDefinition other)
	{
		// FIXME: should be able to compare MethodDefinition references directly
		return self.FullName == other.FullName;
	}

	public static string PropertyName(this MethodDefinition self)
	{
		if (self.Name is null || self.Name.Length < 5)
		{
			throw new ArgumentException(null, nameof(self));
		}

		return self.Name.Value.Substring(4);
	}

	public static bool IsConversionOperator(this MethodDefinition method)
	{
		if (!method.IsSpecialName)
		{
			return false;
		}

		return method.Name == "op_Implicit" || method.Name == "op_Explicit";
	}

	public static bool IsSimpleSetter(this MethodDefinition original)
	{
		return original.IsSetMethod && original.Parameters.Count == 1;
	}

	public static bool IsSimpleGetter(this MethodDefinition original)
	{
		return original.IsGetMethod && original.Parameters.Count == 0;
	}

	public static bool IsSimplePropertyAccessor(this MethodDefinition method)
	{
		return method.IsSimpleGetter() || method.IsSimpleSetter();
	}

	public static bool IsDefaultConstructor(MethodDefinition m)
	{
		return m.IsConstructor && !m.IsStatic && m.Parameters.Count == 0;
	}
}
