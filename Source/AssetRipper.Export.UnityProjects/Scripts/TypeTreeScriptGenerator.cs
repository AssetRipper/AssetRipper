using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Extensions;
using System.Linq;
using System.Text;

namespace AssetRipper.Export.UnityProjects.Scripts;

public static class TypeTreeScriptGenerator
{
	public static string Generate(IMonoScript script, TypeTreeNodeStruct rootNode)
	{
		return Generate(script.Namespace.String, script.ClassName_R.String, rootNode);
	}

	internal static string Generate(MonoScriptInfo script, TypeTreeNodeStruct rootNode)
	{
		return Generate(script.Namespace, script.Class, rootNode);
	}

	public static string Generate(string? @namespace, string className, TypeTreeNodeStruct rootNode)
	{
		string classDeclarationName = GetClassDeclarationName(className);

		StringBuilder sb = new StringBuilder();
		sb.AppendLine("using System;");
		sb.AppendLine("using System.Collections.Generic;");
		sb.AppendLine("using UnityEngine;");
		sb.AppendLine();

		bool hasNamespace = !string.IsNullOrWhiteSpace(@namespace);
		if (hasNamespace)
		{
			sb.Append("namespace ").Append(@namespace).AppendLine();
			sb.AppendLine("{");
		}

		string indent = hasNamespace ? "\t" : string.Empty;
		sb.Append(indent).Append("public class ").Append(classDeclarationName).AppendLine(" : MonoBehaviour");
		sb.Append(indent).AppendLine("{");

		HashSet<string> usedNames = new(StringComparer.Ordinal);
		bool anyFields = false;
		foreach (TypeTreeNodeStruct fieldNode in rootNode.SubNodes)
		{
			if (ShouldSkipField(fieldNode.Name))
			{
				continue;
			}

			string typeName = MapFieldType(fieldNode);
			string fieldName = MakeUniqueIdentifier(SanitizeIdentifier(fieldNode.Name), usedNames);

			sb.Append(indent).Append('\t')
				.Append("public ")
				.Append(typeName)
				.Append(' ')
				.Append(fieldName)
				.AppendLine(";");
			anyFields = true;
		}

		if (!anyFields)
		{
			sb.Append(indent).Append('\t').AppendLine("// No recoverable fields were found in the type tree.");
		}

		sb.Append(indent).AppendLine("}");
		if (hasNamespace)
		{
			sb.AppendLine("}");
		}

		return sb.ToString();
	}

	private static bool ShouldSkipField(string fieldName)
	{
		if (string.IsNullOrEmpty(fieldName))
		{
			return true;
		}

		return fieldName.StartsWith("m_", StringComparison.Ordinal)
			|| fieldName.StartsWith("rg_", StringComparison.Ordinal)
			|| fieldName.StartsWith('<')
			|| fieldName is "references" or "managedReferences";
	}

	private static string MapFieldType(TypeTreeNodeStruct node)
	{
		if (node.IsMap)
		{
			return "Dictionary<object, object>";
		}
		if (node.IsArray || node.IsVector || node.IsNamedVector)
		{
			return "List<object>";
		}
		if (node.IsString)
		{
			return "string";
		}
		if (node.IsPPtr || node.TypeName.StartsWith("PPtr<", StringComparison.Ordinal))
		{
			return "UnityEngine.Object";
		}
		if (node.SubNodes.Count > 0)
		{
			return "object";
		}

		return node.TypeName switch
		{
			"bool" => "bool",
			"char" => "char",
			"SInt8" => "sbyte",
			"UInt8" => "byte",
			"SInt16" or "short" => "short",
			"UInt16" or "ushort" or "unsigned short" => "ushort",
			"SInt32" or "int" => "int",
			"UInt32" or "uint" or "unsigned int" => "uint",
			"SInt64" or "long long" => "long",
			"UInt64" or "unsigned long long" => "ulong",
			"float" => "float",
			"double" => "double",
			"string" => "string",
			_ => "object",
		};
	}

	private static string GetClassDeclarationName(string className)
	{
		if (!MonoScriptExtensions.IsGeneric(className, out string genericName, out int genericCount))
		{
			return SanitizeIdentifier(className);
		}

		string[] genericParameters = Enumerable.Range(1, genericCount).Select(i => $"T{i}").ToArray();
		return $"{SanitizeIdentifier(genericName)}<{string.Join(", ", genericParameters)}>";
	}

	private static string MakeUniqueIdentifier(string baseName, HashSet<string> usedNames)
	{
		string candidate = baseName;
		int suffix = 1;
		while (!usedNames.Add(candidate))
		{
			candidate = $"{baseName}_{suffix}";
			suffix++;
		}
		return candidate;
	}

	private static string SanitizeIdentifier(string identifier)
	{
		if (string.IsNullOrWhiteSpace(identifier))
		{
			return "_field";
		}

		StringBuilder sb = new StringBuilder(identifier.Length + 1);
		for (int i = 0; i < identifier.Length; i++)
		{
			char c = identifier[i];
			if (char.IsLetterOrDigit(c) || c == '_')
			{
				sb.Append(c);
			}
			else
			{
				sb.Append('_');
			}
		}

		if (sb.Length == 0 || char.IsDigit(sb[0]))
		{
			sb.Insert(0, '_');
		}

		string result = sb.ToString();
		return CSharpKeywords.Contains(result) ? $"@{result}" : result;
	}

	private static readonly HashSet<string> CSharpKeywords = new(StringComparer.Ordinal)
	{
		"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
		"continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
		"false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
		"internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
		"params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
		"sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
		"uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
	};
}
