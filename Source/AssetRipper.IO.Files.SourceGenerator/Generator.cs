using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SourceGenerator.Json;

namespace AssetRipper.IO.Files.SourceGenerator;

public static class Generator
{
	internal static void MakeType(IndentedTextWriter writer, TypeDeclaration declaration, TypeDefinition definition, Dictionary<string, string> propertyTypeDictionary)
	{
		bool isReadOnly = declaration.ContainsModifier("readonly");
		bool isStruct = declaration.ContainsModifier("struct");
		bool isRecord = declaration.ContainsModifier("record");
		writer.WriteGeneratedCodeWarning();
		foreach (string @using in declaration.Usings.Append("AssetRipper.IO.Endian").Order())
		{
			writer.WriteUsing(@using);
		}
		writer.WriteFileScopedNamespace(declaration.Namespace);
		MaybeWriteDocumentation(writer, declaration.Summary, declaration.Remarks);
		writer.WriteLine($"public partial {declaration.ClassType} {declaration.Name}_{definition.Version} : I{declaration.Name}");
		using (new CurlyBrackets(writer))
		{
			HashSet<string?> properties = new();
			foreach (FieldDefinition serializableField in definition.SerializableFields)
			{
				MaybeWriteProperty(writer, declaration, propertyTypeDictionary, isReadOnly, serializableField);
				properties.Add(serializableField.FieldName);
			}
			foreach (FieldDefinition extraField in definition.ExtraFields)
			{
				MaybeWriteProperty(writer, declaration, propertyTypeDictionary, isReadOnly, extraField);
				properties.Add(extraField.FieldName);
			}
			foreach ((string propertyName, string propertyType) in propertyTypeDictionary)
			{
				if (!properties.Contains(propertyName))
				{
					declaration.Properties.TryGetValue(propertyName, out PropertyDocumentation? documentation);
					MaybeWriteDocumentation(writer, documentation?.Summary, documentation?.Remarks);
					WriteDefaultProperty(writer, isReadOnly, propertyName, propertyType, documentation);
				}
			}
			WriteReadMethod(writer, definition);
			WriteWriteMethod(writer, definition);
		}
	}

	private static void WriteDefaultProperty(IndentedTextWriter writer, bool isReadOnly, string propertyName, string propertyType, PropertyDocumentation? documentation)
	{
		writer.WriteLine($"public {propertyType} {propertyName}");
		using (new CurlyBrackets(writer))
		{
			if (string.IsNullOrEmpty(documentation?.GetExpression))
			{
				writer.WriteLine("get => default;");
			}
			else
			{
				writer.WriteLine($"get => {documentation.GetExpression};");
			}

			if (!isReadOnly)
			{
				if (string.IsNullOrEmpty(documentation?.SetExpression))
				{
					writer.WriteLine("set { }");
				}
				else
				{
					writer.WriteLine($"set => {documentation.SetExpression};");
				}
			}
		}
		writer.WriteLine();
	}

	private static bool MaybeWriteProperty(IndentedTextWriter writer, TypeDeclaration declaration, Dictionary<string, string> propertyTypeDictionary, bool isReadOnly, FieldDefinition field)
	{
		if (string.IsNullOrEmpty(field.FieldName))
		{
			return false;
		}

		string propertyName = field.FieldName;
		string fieldName = $"m_{propertyName}";
		string fieldType = field.TypeName;

		string propertyType;
		bool castGetAccessor;
		bool castSetAccessor;
		if (field.TypeIsEnum(out string? enumName))
		{
			propertyType = enumName;
			castGetAccessor = true;
			castSetAccessor = true;
		}
		else
		{
			propertyType = propertyTypeDictionary[propertyName];
			castGetAccessor = false; //assume implicit conversions available
			castSetAccessor = propertyType != fieldType;
		}

		string processMethodName = $"On{propertyName}Assignment";

		declaration.TryGetPropertyDocumentation(propertyName, out string? summary, out string? remarks);
		MaybeWriteDocumentation(writer, summary, remarks);
		string defaultValue = field.TypeIsString(out _) ? "string.Empty" : "new()";
		writer.WriteLine(isReadOnly
			? $"private readonly {fieldType} {fieldName} = {defaultValue};"
			: $"private {fieldType} {fieldName} = {defaultValue};");
		writer.WriteLine();

		MaybeWriteDocumentation(writer, summary, remarks);
		writer.WriteLine($"public {propertyType} {propertyName}");
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine(castGetAccessor ? $"get => ({propertyType}){fieldName};" : $"get => {fieldName};");
			if (!isReadOnly)
			{
				writer.WriteLine("set");
				using (new CurlyBrackets(writer))
				{
					writer.WriteLine(castSetAccessor ? $"{fieldName} = ({fieldType})value;" : $"{fieldName} = value;");
					writer.WriteLine($"{processMethodName}(value);");
				}
			}
		}
		writer.WriteLine();

		if (!isReadOnly)
		{
			writer.WriteSummaryDocumentation($"Called when <see cref=\"{propertyName}\"/> is set.");
			writer.WriteLine($"partial void {processMethodName}({propertyType} value);");
			writer.WriteLine();
		}

		return true;
	}

	private static void MaybeWriteDocumentation(IndentedTextWriter writer, string? summary, string? remarks)
	{
		if (!string.IsNullOrEmpty(summary))
		{
			writer.WriteSummaryDocumentation(summary);
		}
		if (!string.IsNullOrEmpty(remarks))
		{
			writer.WriteRemarksDocumentation(remarks);
		}
	}

	private sealed class DummyReadable : IEndianReadable<DummyReadable>
	{
		static DummyReadable IEndianReadable<DummyReadable>.Read(EndianReader reader) => new();
	}

	private static void WriteReadMethod(IndentedTextWriter writer, TypeDefinition definition)
	{
		const string readMethodName = nameof(IEndianReadable<DummyReadable>.Read);
		const string postReadMethodName = $"On{readMethodName}Finished";

		writer.WriteLine($"public void {readMethodName}({nameof(EndianReader)} reader)");
		using (new CurlyBrackets(writer))
		{
			foreach (FieldDefinition serializableField in definition.SerializableFields)
			{
				string assignment = string.IsNullOrEmpty(serializableField.FieldName) ? "" : $"m_{serializableField.FieldName} = ";
				if (serializableField.TypeIsByteAlignment4())
				{
					writer.WriteLine($"reader.{nameof(EndianReader.AlignStream)}();");
				}
				else if (serializableField.TypeIsString(out StringSerialization stringSerialization))
				{
					if (stringSerialization == StringSerialization.NullTerminated)
					{
						writer.WriteLine($"{assignment}reader.{nameof(EndianReader.ReadStringZeroTerm)}();");
					}
					else
					{
						throw new Exception("Can't handle this string type");
					}
				}
				else
				{
					if (PrimitiveHandler.GetTypeNameForKeyword(serializableField.TypeName, out string? typeName))
					{
						writer.WriteLine($"{assignment}reader.Read{typeName}();");
					}
					else
					{
						writer.WriteLine($"{assignment}reader.{nameof(EndianReader.ReadEndian)}<{serializableField.TypeName}>();");
					}
				}
			}
			writer.WriteLine($"{postReadMethodName}(reader);");
		}
		writer.WriteLine();

		writer.WriteSummaryDocumentation($"Called when <see cref=\"{readMethodName}\"/> is finished.");
		writer.WriteLine($"partial void {postReadMethodName}({nameof(EndianReader)} reader);");
		writer.WriteLine();
	}

	private static void WriteWriteMethod(IndentedTextWriter writer, TypeDefinition definition)
	{
		const string writeMethodName = nameof(IEndianWritable.Write);
		const string postWriteMethodName = $"On{writeMethodName}Finished";

		writer.WriteLine($"public void {writeMethodName}({nameof(EndianWriter)} writer)");
		using (new CurlyBrackets(writer))
		{
			foreach (FieldDefinition serializableField in definition.SerializableFields)
			{
				if (serializableField.TypeIsByteAlignment4())
				{
					writer.WriteLine($"writer.{nameof(EndianWriter.AlignStream)}();");
				}
				else if (serializableField.TypeIsString(out StringSerialization stringSerialization))
				{
					if (stringSerialization == StringSerialization.NullTerminated)
					{
						string parameter = string.IsNullOrEmpty(serializableField.FieldName)
						? "\"\""
						: $"m_{serializableField.FieldName}";
						writer.WriteLine($"writer.{nameof(EndianWriter.WriteStringZeroTerm)}({parameter});");
					}
					else
					{
						throw new Exception("Can't handle this string type");
					}
				}
				else
				{
					string parameter = string.IsNullOrEmpty(serializableField.FieldName)
						? $"default({serializableField.TypeName})"
						: $"m_{serializableField.FieldName}";
					if (PrimitiveHandler.GetTypeNameForKeyword(serializableField.TypeName, out string? typeName))
					{
						writer.WriteLine($"writer.{nameof(EndianWriter.Write)}({parameter});");
					}
					else
					{
						writer.WriteLine($"writer.{nameof(EndianWriter.WriteEndian)}({parameter});");
					}
				}
			}
			writer.WriteLine($"{postWriteMethodName}(writer);");
		}
		writer.WriteLine();

		writer.WriteSummaryDocumentation($"Called when <see cref=\"{writeMethodName}\"/> is finished.");
		writer.WriteLine($"partial void {postWriteMethodName}({nameof(EndianWriter)} writer);");
		writer.WriteLine();
	}

	internal static void MakeInterface(IndentedTextWriter writer, TypeDeclaration declaration, Dictionary<string, string> propertyTypeDictionary)
	{
		bool isReadOnly = declaration.ContainsModifier("readonly");
		writer.WriteGeneratedCodeWarning();
		foreach (string @using in declaration.Usings.Append("AssetRipper.IO.Endian").Order())
		{
			writer.WriteUsing(@using);
		}
		writer.WriteFileScopedNamespace(declaration.Namespace);
		MaybeWriteDocumentation(writer, declaration.Summary, declaration.Remarks);
		writer.WriteLine($"public partial interface I{declaration.Name} : {nameof(IEndianWritable)}");
		using (new CurlyBrackets(writer))
		{
			foreach ((string propertyName, string propertyType) in propertyTypeDictionary)
			{
				declaration.TryGetPropertyDocumentation(propertyName, out string? summary, out string? remarks);
				MaybeWriteDocumentation(writer, summary, remarks);
				writer.WriteLine(isReadOnly ? $"public {propertyType} {propertyName} {{ get; }}" : $"public {propertyType} {propertyName} {{ get; set; }}");
				writer.WriteLine();
			}
		}
	}
}
