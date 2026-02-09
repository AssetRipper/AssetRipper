namespace AssetRipper.SerializationLogic.Extensions;

internal static class FieldDefinitionExtensions
{
	public static bool IsConst(this FieldDefinition field)
	{
		return field.IsLiteral && !field.IsInitOnly;
	}
}
