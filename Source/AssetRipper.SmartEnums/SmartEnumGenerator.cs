using AssetRipper.Text.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SGF;
using System.CodeDom.Compiler;

namespace AssetRipper.SmartEnums;

[IncrementalGenerator]
public sealed class SmartEnumGenerator() : IncrementalGenerator(nameof(SmartEnumGenerator))
{
	private readonly record struct SmartEnumData(string Namespace, string StructName, string EnumName, string? EnumType, Sequence<string> EnumValues)
	{
		public IEnumerable<string> GetAllNames()
		{
			yield return StructName;
			yield return EnumName;
			foreach (string enumValue in EnumValues)
			{
				yield return enumValue;
			}
		}
	}

	public override void OnInitialize(SgfInitializationContext context)
	{
		context.RegisterPostInitializationOutput(InjectAttribute);

		IncrementalValuesProvider<SmartEnumData> valueProvider = context.SyntaxProvider.ForAttributeWithMetadataName("AssetRipper.SmartEnums.SmartEnumAttribute",
		static (node, ct) =>
		{
			return node is RecordDeclarationSyntax s
				&& s.IsPartial()
				&& s.IsReadOnly()
				&& s.Parent is BaseNamespaceDeclarationSyntax
				&& s.ChildNodes().OfType<EnumDeclarationSyntax>().Count() == 1;
		},
		static (context, ct) =>
		{
			RecordDeclarationSyntax structDeclaration = (RecordDeclarationSyntax)context.TargetNode;
			BaseNamespaceDeclarationSyntax namespaceDeclaration = (BaseNamespaceDeclarationSyntax)structDeclaration.Parent!;
			EnumDeclarationSyntax enumDeclaration = structDeclaration.ChildNodes().OfType<EnumDeclarationSyntax>().Single();

			string structName = structDeclaration.Identifier.Text;
			string @namespace = namespaceDeclaration.Name.ToString();
			string enumName = enumDeclaration.Identifier.Text;
			string? enumType = enumDeclaration.BaseList?.Types.Single().ToString();
			string[] enumValues = enumDeclaration.Members.OfType<EnumMemberDeclarationSyntax>().Select(e => e.Identifier.Text).ToArray();

			return new SmartEnumData(@namespace, structName, enumName, enumType, enumValues);
		});

		context.RegisterSourceOutput(valueProvider, GenerateEnum);
	}

	private static void GenerateEnum(SgfSourceProductionContext context, SmartEnumData enumData)
	{
		StringWriter stringWriter = new() { NewLine = "\n" };
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(stringWriter);

		string structName = enumData.StructName;
		string enumName = enumData.EnumName;
		string enumType = enumData.EnumType ?? "int";
		string[] enumValues = enumData.EnumValues.Values;

		string valueName = GetNonConflictingName("__value", enumData.GetAllNames());

		writer.WriteGeneratedCodeWarning();
		writer.WriteLineNoTabs();
		writer.WriteFileScopedNamespace(enumData.Namespace);
		writer.WriteLineNoTabs();
		writer.WriteLine($"readonly partial record struct {structName} :");
		using (new Indented(writer))
		{
			writer.WriteLine($"global::System.IParsable<{structName}>,");
			writer.WriteLine($"global::System.Numerics.IBitwiseOperators<{structName}, {structName}, {structName}>,");
			writer.WriteLine($"global::System.Numerics.IComparisonOperators<{structName}, {structName}, bool>,");
			writer.WriteLine($"global::System.Numerics.IEqualityOperators<{structName}, {structName}, bool>,");
			writer.WriteLine($"global::System.Numerics.IShiftOperators<{structName}, int, {structName}>");
		}
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine($"private readonly {enumType} {valueName};");
			writer.WriteLineNoTabs();

			// Constructor
			writer.WriteLine($"public {structName}({enumType} value) => {valueName} = value;");
			writer.WriteLineNoTabs();

			foreach (string enumValue in enumValues)
			{
				writer.WriteInheritDocumentation($"{enumName}.{enumValue}");
				writer.WriteLine($"public const {enumType} {enumValue} = ({enumType}){enumName}.{enumValue};");
				writer.WriteLineNoTabs();
			}

			// Implicit operators
			writer.WriteLine($"public static implicit operator {enumType}({structName} value) => value.{valueName};");
			writer.WriteLine($"public static implicit operator {structName}({enumType} value) => new(value);");
			writer.WriteLineNoTabs();

			// Bitwise operators
			writer.WriteLine($"public static {structName} operator &({structName} left, {structName} right) => new(left.{valueName} & right.{valueName});");
			writer.WriteLine($"public static {structName} operator |({structName} left, {structName} right) => new(left.{valueName} | right.{valueName});");
			writer.WriteLine($"public static {structName} operator ^({structName} left, {structName} right) => new(left.{valueName} ^ right.{valueName});");
			writer.WriteLine($"public static {structName} operator ~({structName} value) => new(~value.{valueName});");
			writer.WriteLineNoTabs();

			// Comparison operators
			writer.WriteLine($"public static bool operator <({structName} left, {structName} right) => left.{valueName} < right.{valueName};");
			writer.WriteLine($"public static bool operator >({structName} left, {structName} right) => left.{valueName} > right.{valueName};");
			writer.WriteLine($"public static bool operator <=({structName} left, {structName} right) => left.{valueName} <= right.{valueName};");
			writer.WriteLine($"public static bool operator >=({structName} left, {structName} right) => left.{valueName} >= right.{valueName};");
			writer.WriteLineNoTabs();

			// Shift operators
			writer.WriteLine($"public static {structName} operator <<({structName} value, int count) => new(value.{valueName} << count);");
			writer.WriteLine($"public static {structName} operator >>({structName} value, int count) => new(value.{valueName} >> count);");
			writer.WriteLine($"public static {structName} operator >>>({structName} value, int count) => new(value.{valueName} >>> count);");
			writer.WriteLineNoTabs();

			// ToString
			writer.WriteLine($"public override string ToString() => {valueName} switch");
			using (new CurlyBracketsWithSemicolon(writer))
			{
				foreach (string enumValue in enumValues)
				{
					writer.WriteLine($"{enumValue} => nameof({enumValue}),");
				}
				writer.WriteLine($"_ => {valueName}.ToString(),");
			}
			writer.WriteLineNoTabs();

			// IParsable
			writer.WriteLine($"public static {structName} Parse(string s) => Parse(s, null);");
			writer.WriteLine($"public static {structName} Parse(string s, IFormatProvider? provider) => s switch");
			using (new CurlyBracketsWithSemicolon(writer))
			{
				foreach (string enumValue in enumValues)
				{
					writer.WriteLine($"nameof({structName}.{enumValue}) => {structName}.{enumValue},");
				}
				writer.WriteLine($"_ => {enumType}.Parse(s, provider),");
			}
			writer.WriteLine($"public static bool TryParse(string? s, out {structName} result) => TryParse(s, null, out result);");
			writer.WriteLine($"public static bool TryParse(string? s, IFormatProvider? provider, out {structName} result)");
			using (new CurlyBrackets(writer))
			{
				writer.WriteLine("switch (s)");
				using (new CurlyBrackets(writer))
				{
					foreach (string enumValue in enumValues)
					{
						writer.WriteLine($"case nameof({structName}.{enumValue}):");
						using (new Indented(writer))
						{
							writer.WriteLine($"result = {structName}.{enumValue};");
							writer.WriteLine("return true;");
						}
					}
					writer.WriteLine("default:");
					using (new Indented(writer))
					{
						writer.WriteLine($"if ({enumType}.TryParse(s, provider, out {enumType} value))");
						using (new CurlyBrackets(writer))
						{
							writer.WriteLine("result = new(value);");
							writer.WriteLine("return true;");
						}
						writer.WriteLine("result = default;");
						writer.WriteLine("return false;");
					}
				}
			}
			writer.WriteLineNoTabs();

			writer.WriteLine($"public static global::System.ReadOnlySpan<{structName}> GetValues() => __ValueCache.Values;");
			writer.WriteLine($"public static global::System.ReadOnlySpan<{enumType}> GetUnderlyingValues() =>");
			writer.WriteLine('[');
			using (new Indented(writer))
			{
				foreach (string enumValue in enumValues)
				{
					writer.WriteLine($"{enumValue},");
				}
			}
			writer.WriteLine("];");
			writer.WriteLine($"public {enumType} GetUnderlyingValue() => {valueName};");
		}
		writer.WriteLine("file static class __ValueCache");
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine($"public static {structName}[] Values = new {structName}[]");
			using (new CurlyBracketsWithSemicolon(writer))
			{
				foreach (string enumValue in enumValues)
				{
					writer.WriteLine($"{structName}.{enumValue},");
				}
			}
		}

		context.AddSource($"{structName}.cs", stringWriter.ToString());
	}

	private static void InjectAttribute(IncrementalGeneratorPostInitializationContext context)
	{
		context.AddSource("SmartEnumAttribute.cs", """
			namespace AssetRipper.SmartEnums;

			//[global::Microsoft.CodeAnalysis.Embedded]
			[global::System.AttributeUsage(global::System.AttributeTargets.Struct)]
			internal sealed class SmartEnumAttribute : global::System.Attribute
			{
			}
			""");

		//context.AddEmbeddedAttributeDefinition(); //To do: this api isn't available yet
		//https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md#put-microsoftcodeanalysisembeddedattribute-on-generated-marker-types
	}

	private static string GetNonConflictingName(string name, IEnumerable<string> existingNames)
	{
		while (existingNames.Contains(name))
		{
			name = "_" + name;
		}
		return name;
	}
}
