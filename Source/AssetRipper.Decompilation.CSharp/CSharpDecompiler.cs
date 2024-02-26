using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures.Types;
using AssetRipper.Text.SourceGeneration;
using System.CodeDom.Compiler;

namespace AssetRipper.Decompilation.CSharp;

public class CSharpDecompiler : 
	IVisitor<TypeDefinition, (IndentedTextWriter, NameGenerator), IndentedTextWriter>,
	IVisitor<FieldDefinition, (IndentedTextWriter, NameGenerator), IndentedTextWriter>,
	IVisitor<MethodDefinition, (IndentedTextWriter, NameGenerator), IndentedTextWriter>,
	IVisitor<CilMethodBody, (IndentedTextWriter, NameGenerator), IndentedTextWriter>,
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

	public IndentedTextWriter Visit(TypeDefinition type, (IndentedTextWriter, NameGenerator) state)
	{
		IndentedTextWriter writer = state.Item1;
		NameGenerator nameGenerator = state.Item2;

		if (!Utf8String.IsNullOrEmpty(type.Namespace))
		{
			writer.WriteFileScopedNamespace(type.Namespace);
		}

		VisitCustomAttributes(type, state);

		writer.Write(type.GetAccessModifier());
		writer.Write(' ');

		if (type.TryGetInheritanceModifier(out string? inheritanceModifier))
		{
			writer.Write(inheritanceModifier);
			writer.Write(' ');
		}

		writer.Write(type.GetTypeCategory());
		writer.Write(' ');

		writer.Write(type.Name);

		if (type.BaseType is not null && !IsSpecialType(type.BaseType))
		{
			writer.Write(" : ");
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
			foreach (FieldDefinition field in type.Fields)
			{
				Visit(field, state)
					.WriteLineNoTabs();
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
		(IndentedTextWriter writer, NameGenerator nameGenerator) = state;

		if (field.IsCompilerGenerated())
		{
			writer.WriteComment("Compiler generated field");
		}

		VisitCustomAttributes(field, state);

		writer.Write(field.GetAccessModifier());
		writer.Write(' ');

		if (field.IsStatic)
		{
			writer.Write("static ");
		}

		if (field.Signature is null)
		{
			writer.Write("/* Could not determine field type */ ");
		}
		else
		{
			writer.Write(nameGenerator.GetFullName(field.Signature.FieldType));
			writer.Write(' ');
		}

		writer.Write(field.Name);
		writer.WriteLine(';');

		return writer;
	}

	public IndentedTextWriter Visit(MethodDefinition method, (IndentedTextWriter, NameGenerator) state)
	{
		(IndentedTextWriter writer, NameGenerator nameGenerator) = state;

		VisitCustomAttributes(method, state);

		if (!method.IsStatic || !method.IsConstructor)
		{
			writer.Write(method.GetAccessModifier());
			writer.Write(' ');
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

		writer.Write(')');

		if (method.CilMethodBody is null)
		{
			writer.WriteLine(';');
		}
		else
		{
			writer.WriteLine();
			using (new CurlyBrackets(writer))
			{
				Visit(method.CilMethodBody, state);
			}
		}

		return writer;
	}

	public IndentedTextWriter Visit(CilMethodBody body, (IndentedTextWriter, NameGenerator) state)
	{
		(IndentedTextWriter writer, NameGenerator nameGenerator) = state;

		writer.WriteComment("Method body decompilation not implemented yet");
		writer.WriteLine("throw null;");

		return writer;
	}

	private void VisitCustomAttributes(IHasCustomAttribute owner, (IndentedTextWriter, NameGenerator) state)
	{
		(IndentedTextWriter writer, NameGenerator nameGenerator) = state;

		if (owner.CustomAttributes.Count > 0)
		{
			writer.WriteComment("Custom attribute decompilation not implemented yet");
		}
	}

	public IndentedTextWriter Visit(EventDefinition @event, (IndentedTextWriter, NameGenerator) state)
	{
		throw new NotImplementedException();
	}

	public IndentedTextWriter Visit(PropertyDefinition property, (IndentedTextWriter, NameGenerator) state)
	{
		(IndentedTextWriter writer, NameGenerator nameGenerator) = state;

		MethodDefinition? getMethod = property.GetMethod;
		MethodDefinition? setMethod = property.SetMethod;

		VisitCustomAttributes(property, state);

		string propertyAccessModifier = property.GetAccessModifier();
		writer.Write(propertyAccessModifier);
		writer.Write(' ');

		if (getMethod?.IsStatic ?? setMethod?.IsStatic ?? false)
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
			if (getMethod is not null)
			{
				string getAccessModifier = getMethod.GetAccessModifier();
				if (getAccessModifier != propertyAccessModifier)
				{
					writer.Write(getAccessModifier);
					writer.Write(' ');
				}
				if (getMethod.CilMethodBody is null)
				{
					writer.WriteLine("get => default;");
				}
				else
				{
					writer.WriteLine("get");
					using (new CurlyBrackets(writer))
					{
						Visit(getMethod.CilMethodBody, state);
					}
				}
			}
			if (setMethod is not null)
			{
				string setAccessModifier = setMethod.GetAccessModifier();
				if (setAccessModifier != propertyAccessModifier)
				{
					writer.Write(setAccessModifier);
					writer.Write(' ');
				}
				if (setMethod.CilMethodBody is null)
				{
					writer.WriteLine("set { }");
				}
				else
				{
					writer.WriteLine("set");
					using (new CurlyBrackets(writer))
					{
						Visit(setMethod.CilMethodBody, state);
					}
				}
			}
		}

		return writer;
	}
}
