using AsmResolver.DotNet;
using AssetRipper.Text.SourceGeneration;
using System.CodeDom.Compiler;

namespace AssetRipper.Decompilation.CSharp;

public static class CSharpDecompiler
{
	public static string Decompile(TypeDefinition type)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter textWriter = new(stringWriter);

		textWriter.WriteComment("This decompilation assumes that the latest C# version is being used.");
		if (type.Namespace is not null)
		{
			textWriter.WriteFileScopedNamespace(type.Namespace);
		}

		textWriter.Write(GetAccessModifier(type));
		textWriter.Write(' ');

		if (TryGetInheritanceModifier(type, out string? inheritanceModifier))
		{
			textWriter.Write(inheritanceModifier);
			textWriter.Write(' ');
		}
		
		textWriter.Write(GetTypeCategory(type));
		textWriter.Write(' ');

		textWriter.Write(type.Name);

		if (type.BaseType is not null && !IsSpecialType(type.BaseType))
		{
			textWriter.Write(" : global::");
			textWriter.Write(type.BaseType.FullName);
		}
		textWriter.WriteLine();
		using (new CurlyBrackets(textWriter))
		{
			textWriter.WriteComment("Member decompilation not implemented yet");
		}
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
}
