using System.Diagnostics.CodeAnalysis;

namespace AssemblyDumper.Utils
{
	public static class FieldUtils
	{
		public static List<FieldDefinition> GetAllFieldsInTypeAndBase(this TypeDefinition? type)
		{
			if (type == null)
				return new();

			List<FieldDefinition>? ret = type.Fields.ToList();

			ret.AddRange(GetAllFieldsInTypeAndBase(type.BaseType?.Resolve()));

			return ret;
		}

		public static FieldDefinition GetFieldByName(this TypeDefinition type, string fieldName, bool checkBaseTypes = false)
		{
			return type.TryGetFieldByName(fieldName, checkBaseTypes)
				?? throw new Exception($"{type.FullName} doesn't have a {fieldName} field");
		}

		public static bool TryGetFieldByName(this TypeDefinition type, string fieldName, [NotNullWhen(true)][MaybeNullWhen(false)] out FieldDefinition? field, bool checkBaseTypes = false)
		{
			field = type.TryGetFieldByName(fieldName, checkBaseTypes);
			return field != null;
		}

		public static FieldDefinition? TryGetFieldByName(this TypeDefinition type, string fieldName, bool checkBaseTypes = false)
		{
			var result = type.Fields.SingleOrDefault(field => field.Name == fieldName);
			if(result == null && checkBaseTypes)
			{
				result = type.TryGetBaseTypeDefinition()?.TryGetFieldByName(fieldName, checkBaseTypes);
			}
			return result;
		}

		private static TypeDefinition? TryGetBaseTypeDefinition(this TypeDefinition type)
		{
			return type.BaseType as TypeDefinition;
		}
	}
}