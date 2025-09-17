using System.Xml;

namespace AssetRipper.AssemblyDumper.Documentation;

internal static class XmlUtils
{
	internal static XmlDocument CreateNewDocument()
	{
		XmlDocument document = new XmlDocument();
		document.AppendChild(document.CreateXmlDeclaration("1.0", null, null));
		return document;
	}

	internal static string EscapeXmlInvalidCharacters(string originalString)
	{
		return System.Security.SecurityElement.Escape(originalString);
	}

	internal static string GetStringReference(Type type)
	{
		return $"T:{type.FullName}";
	}

	internal static string GetStringReference(TypeDefinition type)
	{
		return $"T:{type.FullName}";
	}

	internal static string GetStringReference(PropertyDefinition property)
	{
		return $"P:{property.DeclaringType?.FullName}.{property.Name}";
	}

	//If the name of the item itself has periods, they're replaced by the hash-sign ('#').
	//It's assumed that no item has a hash-sign directly in its name.
	//For example, the fully qualified name of the String constructor is "System.String.#ctor".
	internal static string GetStringReference(FieldDefinition field)
	{
		return $"F:{field.DeclaringType?.FullName}.{((string?)field.Name)?.Replace('.', '#')}";
	}

	internal static string GetStringReference(EventDefinition @event)
	{
		return $"E:{@event.DeclaringType?.FullName}.{@event.Name}";
	}

	internal static string GetStringReference(MethodDefinition method)
	{
		//@ is used instead of & for by-ref parameters.
		return $"M:{method.DeclaringType?.FullName}.{method.Name}{GetParameterString(method.Signature)}".Replace('&', '@');

		static string GetParameterString(MethodSignature? signature)
		{
			return signature is null || signature.ParameterTypes.Count == 0
				? string.Empty
				: $"({string.Join(',', signature.ParameterTypes.Select(t => t.FullName))})";
		}
	}

	internal static string GetStringReference(string @namespace)
	{
		return $"N:{@namespace}";
	}
}
