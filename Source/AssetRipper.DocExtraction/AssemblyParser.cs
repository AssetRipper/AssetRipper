using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AssetRipper.DocExtraction.Extensions;
using AssetRipper.DocExtraction.MetaData;

namespace AssetRipper.DocExtraction;

public static class AssemblyParser
{
	private static readonly HashSet<string?> ClassBlackList = new()
	{
		"<Module>",
		"<PrivateImplementationDetails>",
		"System.Attribute",
		"System.Exception",
		"System.IO.Stream",
		"System.SystemException",
		"UnityEditor.Build.BuildPlayerProcessor",
		"UnityEditor.Experimental.AssetsModifiedProcessor",
		"UnityEditor.AssetModificationProcessor",
		"UnityEditor.AssetPostinstructions",
		"UnityEditor.Editor",
		"UnityEditor.EditorWindow",
		"UnityEditor.Joint2DEditor",
		"UnityEngine.PropertyAttribute",
		"UnityEditor.RendererEditorBase",
	};

	public static void ExtractDocumentationFromAssembly(
		string dllPath,
		Dictionary<string, string> typeSummaries,
		Dictionary<string, string> fieldSummaries,
		Dictionary<string, string> propertySummaries,
		Dictionary<string, ClassDocumentation> classDictionary,
		Dictionary<string, EnumDocumentation> enumDictionary,
		Dictionary<string, StructDocumentation> structDictionary)
	{
		ModuleDefinition module = ModuleDefinition.FromFile(dllPath);
		foreach (TypeDefinition type in module.TopLevelTypes)
		{
			ExtractDocumentationFromType(type, typeSummaries, fieldSummaries, propertySummaries, classDictionary, enumDictionary, structDictionary);
		}
	}

	private static void ExtractDocumentationFromType(TypeDefinition type, Dictionary<string, string> typeSummaries, Dictionary<string, string> fieldSummaries, Dictionary<string, string> propertySummaries, Dictionary<string, ClassDocumentation> classDictionary, Dictionary<string, EnumDocumentation> enumDictionary, Dictionary<string, StructDocumentation> structDictionary)
	{
		string typeFullName = type.FullName;
		if (type.GenericParameters.Count > 0 || ClassBlackList.Contains(typeFullName) || ClassBlackList.Contains(type.BaseType?.FullName) || type.IsCompilerGenerated())
		{
			return;
		}
		else if (type.IsEnum)
		{
			EnumDocumentation enumDocumentation = AddEnumDocumentation(typeSummaries, fieldSummaries, type, typeFullName);
			enumDictionary.Add(typeFullName, enumDocumentation);
		}
		else if (type.IsValueType)
		{
			StructDocumentation structDocumentation = AddStructDocumentation(typeSummaries, fieldSummaries, propertySummaries, type, typeFullName);
			structDictionary.Add(typeFullName, structDocumentation);
		}
		else if (!type.IsInterface && !type.IsStatic())
		{
			ClassDocumentation classDocumentation = AddClassDocumetation(typeSummaries, fieldSummaries, propertySummaries, type, typeFullName);
			classDictionary.Add(typeFullName, classDocumentation);
		}

		foreach (TypeDefinition nestedType in type.NestedTypes)
		{
			ExtractDocumentationFromType(nestedType, typeSummaries, fieldSummaries, propertySummaries, classDictionary, enumDictionary, structDictionary);
		}
	}

	//Static fields and properties are included because some editor classes have important properties as static.

	private static ClassDocumentation AddClassDocumetation(Dictionary<string, string> typeSummaries, Dictionary<string, string> fieldSummaries, Dictionary<string, string> propertySummaries, TypeDefinition type, string typeFullName)
	{
		ClassDocumentation classDocumentation = new()
		{
			Name = type.Name ?? throw new NullReferenceException("Name cannot be null"),
			FullName = typeFullName,
			BaseName = type.BaseType?.Name is null ? "Object" : type.BaseType.Name,
			BaseFullName = type.BaseType?.Name is null ? "System.Object" : type.BaseType.FullName,
			DocumentationString = typeSummaries.TryGetValue(typeFullName.Replace('+', '.')),
			ObsoleteMessage = type.GetObsoleteMessage(),
			NativeName = type.GetNativeClass(),
		};

		foreach (FieldDefinition field in type.Fields)
		{
			if (!field.IsCompilerGenerated())
			{
				string fieldName = field.Name ?? throw new NullReferenceException("Field Name cannot be null");
				DataMemberDocumentation fieldDocumentation = new()
				{
					Name = fieldName,
					TypeName = field.Signature?.FieldType.Name ?? throw new NullReferenceException("Field Signature cannot be null"),
					TypeFullName = field.Signature?.FieldType.FullName ?? throw new NullReferenceException("Field Signature cannot be null"),
					DocumentationString = fieldSummaries.TryGetValue($"{typeFullName.Replace('+', '.')}.{field.Name}"),
					ObsoleteMessage = field.GetObsoleteMessage(),
					NativeName = field.GetNativeName(),
				};
				classDocumentation.Members.Add(fieldName, fieldDocumentation);
			}
		}

		foreach (PropertyDefinition property in type.Properties)
		{
			if (!property.HasParameters() && !property.IsCompilerGenerated())
			{
				string propertyName = property.Name ?? throw new NullReferenceException("Property Name cannot be null");
				DataMemberDocumentation propertyDocumentation = new()
				{
					Name = propertyName,
					TypeName = property.Signature?.ReturnType?.Name ?? throw new NullReferenceException("Property Type cannot be null"),
					TypeFullName = property.Signature?.ReturnType?.FullName ?? throw new NullReferenceException("Property Type cannot be null"),
					DocumentationString = propertySummaries.TryGetValue($"{typeFullName.Replace('+', '.')}.{property.Name}"),
					ObsoleteMessage = property.GetObsoleteMessage(),
					NativeName = property.GetNativeName() ?? property.GetNativeProperty(),
				};
				classDocumentation.Members.Add(propertyName, propertyDocumentation);
			}
		}

		return classDocumentation;
	}

	private static EnumDocumentation AddEnumDocumentation(Dictionary<string, string> typeSummaries, Dictionary<string, string> fieldSummaries, TypeDefinition type, string typeFullName)
	{
		FieldDefinition valueField = type.Fields[0]; //value field is always first
		if (valueField.IsStatic)
		{
			throw new Exception("Value field can't be static");
		}

		EnumDocumentation enumDocumentation = new()
		{
			ElementType = ((CorLibTypeSignature)valueField.Signature!.FieldType).ElementType,
			IsFlagsEnum = type.HasAttribute("System", nameof(FlagsAttribute)),
			Name = type.Name ?? throw new NullReferenceException("Type Name cannot be null"),
			FullName = typeFullName,
			DocumentationString = typeSummaries.TryGetValue(typeFullName.Replace('+', '.')),
			ObsoleteMessage = type.GetObsoleteMessage(),
			NativeName = null,//NativeClassAttribute isn't valid on enums
		};

		for (int i = 1; i < type.Fields.Count; i++)
		{
			FieldDefinition enumField = type.Fields[i];
			if (enumField.IsStatic)
			{
				string enumFieldName = enumField.Name ?? throw new NullReferenceException("Field Name cannot be null");
				EnumMemberDocumentation memberDocumentation = new()
				{
					Name = enumFieldName,
					Value = enumField.Constant!.ConvertToLong(),
					DocumentationString = fieldSummaries.TryGetValue($"{typeFullName.Replace('+', '.')}.{enumField.Name}"),
					ObsoleteMessage = enumField.GetObsoleteMessage(),
					NativeName = enumField.GetNativeName(),
				};
				enumDocumentation.Members.Add(enumFieldName, memberDocumentation);
			}
			else
			{
				throw new Exception("Enum field must be static");
			}
		}

		return enumDocumentation;
	}

	private static StructDocumentation AddStructDocumentation(Dictionary<string, string> typeSummaries, Dictionary<string, string> fieldSummaries, Dictionary<string, string> propertySummaries, TypeDefinition type, string typeFullName)
	{
		StructDocumentation structDocumentation = new()
		{
			Name = type.Name ?? throw new NullReferenceException("Name cannot be null"),
			FullName = typeFullName,
			DocumentationString = typeSummaries.TryGetValue(typeFullName.Replace('+', '.')),
			ObsoleteMessage = type.GetObsoleteMessage(),
			NativeName = type.GetNativeClass(),
		};

		foreach (FieldDefinition field in type.Fields)
		{
			if (!field.IsCompilerGenerated())
			{
				string fieldName = field.Name ?? throw new NullReferenceException("Field Name cannot be null");
				DataMemberDocumentation fieldDocumentation = new()
				{
					Name = fieldName,
					TypeName = field.Signature?.FieldType.Name ?? throw new NullReferenceException("Field Signature cannot be null"),
					TypeFullName = field.Signature?.FieldType.FullName ?? throw new NullReferenceException("Field Signature cannot be null"),
					DocumentationString = fieldSummaries.TryGetValue($"{typeFullName.Replace('+', '.')}.{field.Name}"),
					ObsoleteMessage = field.GetObsoleteMessage(),
					NativeName = field.GetNativeName(),
				};
				structDocumentation.Members.Add(fieldName, fieldDocumentation);
			}
		}

		foreach (PropertyDefinition property in type.Properties)
		{
			if (!property.HasParameters() && !property.IsCompilerGenerated())
			{
				string propertyName = property.Name ?? throw new NullReferenceException("Property Name cannot be null");
				DataMemberDocumentation propertyDocumentation = new()
				{
					Name = propertyName,
					TypeName = property.Signature?.ReturnType?.Name ?? throw new NullReferenceException("Property Type cannot be null"),
					TypeFullName = property.Signature?.ReturnType?.FullName ?? throw new NullReferenceException("Property Type cannot be null"),
					DocumentationString = propertySummaries.TryGetValue($"{typeFullName.Replace('+', '.')}.{property.Name}"),
					ObsoleteMessage = property.GetObsoleteMessage(),
					NativeName = property.GetNativeName() ?? property.GetNativeProperty(),
				};
				structDocumentation.Members.Add(propertyName, propertyDocumentation);
			}
		}

		return structDocumentation;
	}

	private static bool IsCompilerGenerated(this IHasCustomAttribute hasCustomAttribute)
	{
		return hasCustomAttribute.HasAttribute("System.Runtime.CompilerServices", nameof(System.Runtime.CompilerServices.CompilerGeneratedAttribute));
	}
}