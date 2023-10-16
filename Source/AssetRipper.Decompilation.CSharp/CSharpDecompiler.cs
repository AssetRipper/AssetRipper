using AsmResolver;
using AsmResolver.DotNet;
using AssetRipper.Text.SourceGeneration;
using System.CodeDom.Compiler;

namespace AssetRipper.Decompilation.CSharp;

public class CSharpDecompiler : IDefinitionVisitor<IndentedTextWriter, IndentedTextWriter>
{
	public static string Decompile(TypeDefinition type)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter textWriter = new(stringWriter);
		textWriter.WriteComment("This decompilation assumes that the latest C# version is being used.");
		type.AcceptVisitor(new CSharpDecompiler(), textWriter);
		return stringWriter.ToString();
	}

	private static bool IsSpecialType(ITypeDefOrRef type)
	{
		if (type.Namespace == "System")
		{
			if (type.Name?.Value is "Object" or "ValueType" or "Enum" or "Delegate")
			{
				return type.Scope?.GetAssembly()?.IsCorLib ?? false;
			}
		}
		return false;
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

	IndentedTextWriter IDefinitionVisitor<IndentedTextWriter, IndentedTextWriter>.VisitType(TypeDefinition type, IndentedTextWriter state)
	{
		NameGenerator nameGenerator = TypeScopedNameGenerator.Create(type);
		if (!Utf8String.IsNullOrEmpty(type.Namespace))
		{
			state.WriteFileScopedNamespace(type.Namespace);
		}

		state.Write(GetAccessModifier(type));
		state.Write(' ');

		if (TryGetInheritanceModifier(type, out string? inheritanceModifier))
		{
			state.Write(inheritanceModifier);
			state.Write(' ');
		}

		state.Write(GetTypeCategory(type));
		state.Write(' ');

		state.Write(type.Name);

		if (type.BaseType is not null && !IsSpecialType(type.BaseType))
		{
			state.Write(nameGenerator.GetFullName(type.BaseType.ToTypeSignature()));
		}
		state.WriteLine();
		using (new CurlyBrackets(state))
		{
			foreach (TypeDefinition nestedType in type.NestedTypes)
			{
				nestedType.AcceptVisitor(this, state)
					.WriteLineNoTabs();
			}
			if (type.Fields.Count > 0)
			{
				state.WriteComment("Field decompilation not implemented yet");
				state.WriteLineNoTabs();
			}
			if (type.Methods.Count > 0)
			{
				state.WriteComment("Method decompilation not implemented yet");
				state.WriteLineNoTabs();
			}
			if (type.Properties.Count > 0)
			{
				state.WriteComment("Property decompilation not implemented yet");
				state.WriteLineNoTabs();
			}
			if (type.Events.Count > 0)
			{
				state.WriteComment("Event decompilation not implemented yet");
				state.WriteLineNoTabs();
			}
		}
		return state;
	}

	IndentedTextWriter IDefinitionVisitor<IndentedTextWriter, IndentedTextWriter>.VisitField(FieldDefinition field, IndentedTextWriter state)
	{
		throw new NotImplementedException();
	}

	IndentedTextWriter IDefinitionVisitor<IndentedTextWriter, IndentedTextWriter>.VisitMethod(MethodDefinition method, IndentedTextWriter state)
	{
		throw new NotImplementedException();
	}

	IndentedTextWriter IDefinitionVisitor<IndentedTextWriter, IndentedTextWriter>.VisitEvent(EventDefinition @event, IndentedTextWriter state)
	{
		throw new NotImplementedException();
	}

	IndentedTextWriter IDefinitionVisitor<IndentedTextWriter, IndentedTextWriter>.VisitProperty(PropertyDefinition property, IndentedTextWriter state)
	{
		throw new NotImplementedException();
	}
}
