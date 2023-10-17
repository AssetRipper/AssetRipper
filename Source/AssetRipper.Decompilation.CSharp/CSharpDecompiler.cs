using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures.Types;
using AssetRipper.Text.SourceGeneration;
using System.CodeDom.Compiler;

namespace AssetRipper.Decompilation.CSharp;

public class CSharpDecompiler : 
	IVisitor<TypeDefinition, (IndentedTextWriter, NameGenerator), IndentedTextWriter>,
	IVisitor<FieldDefinition, (IndentedTextWriter, NameGenerator), IndentedTextWriter>,
	IVisitor<MethodDefinition, (IndentedTextWriter, NameGenerator), IndentedTextWriter>,
	IVisitor<EventDefinition, (IndentedTextWriter, NameGenerator), IndentedTextWriter>,
	IVisitor<PropertyDefinition, (IndentedTextWriter, NameGenerator), IndentedTextWriter>
{
	public static string Decompile(TypeDefinition type)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter textWriter = new(stringWriter);
		textWriter.WriteComment("This decompilation assumes that the latest C# version is being used.");
		new CSharpDecompiler().Visit(type, (textWriter, TypeScopedNameGenerator.Create(type)));
		return stringWriter.ToString();
	}

	private static bool IsSpecialType(ITypeDefOrRef type)
	{
		return type.DeclaringType is null
			&& type.Namespace == "System"
			&& type.Name?.Value is "Object" or "ValueType" or "Enum" or "Delegate"
			&& (type.Scope?.GetAssembly()?.IsCorLib ?? false);
	}

	private static string GetAccessModifier(TypeDefinition type)
	{
		if (type.IsNested)
		{
			if (type.IsNestedPublic)
			{
				return "public";
			}
			else if (type.IsNestedAssembly)
			{
				return "internal";
			}
			else if (type.IsNestedFamily)
			{
				return "protected";
			}
			else if (type.IsNestedPrivate)
			{
				return "private";
			}
			else if (type.IsNestedFamilyOrAssembly)
			{
				return "protected internal";
			}
			else if (type.IsNestedFamilyAndAssembly)
			{
				return "private protected";
			}
		}
		else
		{
			if (type.IsPublic)
			{
				return "public";
			}
			else if (type.IsNotPublic)
			{
				return "internal";
			}
		}
		throw new NotSupportedException($"Unsupported access modifier for type {type.FullName}");
	}

	private static string GetTypeCategory(TypeDefinition type)
	{
		if (type.IsClass)
		{
			return "class";
		}
		else if (type.IsInterface)
		{
			return "interface";
		}
		else if (type.IsEnum)
		{
			return "enum";
		}
		else if (type.IsValueType)
		{
			return "struct";
		}
		else if (type.IsDelegate)
		{
			return "delegate";
		}
		throw new NotSupportedException($"Unsupported type category for type {type.FullName}");
	}

	private static bool TryGetInheritanceModifier(TypeDefinition type, [NotNullWhen(true)] out string? modifier)
	{
		if (type.IsSealed)
		{
			modifier = type.IsAbstract ? "static" : "sealed";
			return true;
		}
		else if (type.IsAbstract)
		{
			modifier = "abstract";
			return true;
		}
		modifier = null;
		return false;
	}

	public IndentedTextWriter Visit(TypeDefinition type, (IndentedTextWriter, NameGenerator) state)
	{
		IndentedTextWriter writer = state.Item1;
		NameGenerator nameGenerator = state.Item2;

		if (!Utf8String.IsNullOrEmpty(type.Namespace))
		{
			writer.WriteFileScopedNamespace(type.Namespace);
		}

		writer.Write(GetAccessModifier(type));
		writer.Write(' ');

		if (TryGetInheritanceModifier(type, out string? inheritanceModifier))
		{
			writer.Write(inheritanceModifier);
			writer.Write(' ');
		}

		writer.Write(GetTypeCategory(type));
		writer.Write(' ');

		writer.Write(type.Name);

		if (type.BaseType is not null && !IsSpecialType(type.BaseType))
		{
			writer.Write(nameGenerator.GetFullName(type.BaseType.ToTypeSignature()));
		}
		writer.WriteLine();
		using (new CurlyBrackets(writer))
		{
			foreach (TypeDefinition nestedType in type.NestedTypes)
			{
				Visit(nestedType, (writer, TypeScopedNameGenerator.Create(nestedType)))
					.WriteLineNoTabs();
			}
			if (type.Fields.Count > 0)
			{
				writer.WriteComment("Field decompilation not implemented yet");
				writer.WriteLineNoTabs();
			}
			foreach (MethodDefinition method in type.Methods.Where(m => m.Semantics is null))
			{
				Visit(method, (writer, MethodScopedNameGenerator.Create(method)))
					.WriteLineNoTabs();
			}
			foreach (PropertyDefinition property in type.Properties)
			{
				Visit(property, state)
					.WriteLineNoTabs();
			}
			if (type.Events.Count > 0)
			{
				writer.WriteComment("Event decompilation not implemented yet");
				writer.WriteLineNoTabs();
			}
		}
		return writer;
	}

	public IndentedTextWriter Visit(FieldDefinition field, (IndentedTextWriter, NameGenerator) state)
	{
		throw new NotImplementedException();
	}

	public IndentedTextWriter Visit(MethodDefinition method, (IndentedTextWriter, NameGenerator) state)
	{
		IndentedTextWriter writer = state.Item1;
		NameGenerator nameGenerator = state.Item2;

		if (!method.IsStatic || !method.IsConstructor)
		{
			writer.WriteComment("Accessibly modifier not implemented yet");
			writer.Write("public ");
		}

		if (method.IsStatic)
		{
			writer.Write("static ");
		}

		if (method.IsConstructor)
		{
			if (method.DeclaringType is not null)
			{
				writer.Write(method.DeclaringType.Name);
			}
			else
			{
				writer.Write("/* Could not determine type name */");
			}
		}
		else if (method.Signature is null)
		{
			writer.Write("/* Could not determine return type */ ");
			writer.Write(method.Name);
		}
		else
		{
			writer.Write(nameGenerator.GetFullName(method.Signature.ReturnType));
			writer.Write(' ');
			writer.Write(method.Name);
		}

		if (method.GenericParameters.Count > 0)
		{
			writer.Write('<');
			for (int i = 0; i < method.GenericParameters.Count; i++)
			{
				if (i > 0)
				{
					writer.Write(", ");
				}
				writer.Write(nameGenerator.GetFullName(new GenericParameterSignature(GenericParameterType.Method, method.GenericParameters[i].Number)));
			}
			writer.Write('>');
		}

		writer.Write('(');

		for (int i = 0; i < method.Parameters.Count; i++)
		{
			if (i > 0)
			{
				writer.Write(", ");
			}
			Parameter parameter = method.Parameters[i];
			writer.Write(nameGenerator.GetFullName(parameter.ParameterType));
			writer.Write(' ');
			writer.Write(parameter.Definition?.Name ?? $"parameter_{i + 1}");
			//Parameter name stripping is rare. We will worry about name conflicts later.
		}

		writer.WriteLine(')');

		using (new CurlyBrackets(writer))
		{
			writer.WriteComment("Method body decompilation not implemented yet");
			writer.WriteLine("throw null;");
		}

		return writer;
	}

	public IndentedTextWriter Visit(EventDefinition @event, (IndentedTextWriter, NameGenerator) state)
	{
		throw new NotImplementedException();
	}

	public IndentedTextWriter Visit(PropertyDefinition property, (IndentedTextWriter, NameGenerator) state)
	{
		IndentedTextWriter writer = state.Item1;
		NameGenerator nameGenerator = state.Item2;

		writer.WriteComment("Accessibly modifier not implemented yet");
		writer.Write("public ");

		if (property.GetMethod?.IsStatic ?? property.SetMethod?.IsStatic ?? false)
		{
			writer.Write("static ");
		}

		if (property.Signature is null)
		{
			writer.Write("/* Could not determine property type */ ");
		}
		else
		{
			writer.Write(nameGenerator.GetFullName(property.Signature.ReturnType));
			writer.Write(' ');
		}

		writer.WriteLine(property.Name);
		using (new CurlyBrackets(writer))
		{
			writer.WriteComment("Accessor decompilation not implemented yet.");
			if (property.GetMethod is not null)
			{
				writer.WriteLine("get => default;");
			}
			if (property.SetMethod is not null)
			{
				writer.WriteLine("set { }");
			}
		}

		return writer;
	}
}
