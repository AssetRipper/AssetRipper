using AssetRipper.Text.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SGF;
using System.CodeDom.Compiler;
using System.Collections.Immutable;

namespace AssetRipper.GUI.Licensing.SourceGenerator;

[IncrementalGenerator]
public sealed class LicensesGenerator() : IncrementalGenerator(nameof(LicensesGenerator))
{
	public override void OnInitialize(SgfInitializationContext context)
	{
		IncrementalValueProvider<ImmutableArray<(string Name, string License)>> pipeline = context.AdditionalTextsProvider
			.Where(static (text) => text.Path.EndsWith(".md"))
			.Select(static (text, cancellationToken) =>
			{
				string name = Path.GetFileNameWithoutExtension(text.Path);
				string? license = text.GetText(cancellationToken)?.ToString();
				return (name, license);
			})
			.Where(static (pair) => pair.license != null)
			.Collect()!;

		context.RegisterSourceOutput(pipeline, GenerateCode);
	}

	private static void GenerateCode(SgfSourceProductionContext context, ImmutableArray<(string Name, string License)> array)
	{
		using StringWriter stringWriter = new();
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(stringWriter);

		writer.WriteGeneratedCodeWarning();
		writer.WriteLineNoTabs();

		writer.WriteLine("#nullable enable");
		writer.WriteLineNoTabs();
		writer.WriteFileScopedNamespace("AssetRipper.GUI.Licensing");
		writer.WriteLineNoTabs();
		writer.WriteLine("partial class Licenses");
		using (new CurlyBrackets(writer))
		{
			List<(string, string)> sourceNames = new(array.Length);
			foreach ((string name, string license) in array.OrderBy(pair => pair.Name))
			{
				string sourceName = name.Replace('.', '_').Replace('-', '_');
				sourceNames.Add((name, sourceName));
				writer.WriteLine($"private static global::System.ReadOnlySpan<byte> {sourceName}_span => {SymbolDisplay.FormatLiteral(license, true)}u8;");
				writer.WriteLine($"private static string? {sourceName}_field;");
				if (name != sourceName)
				{
					writer.WriteSummaryDocumentation(name);
				}
				writer.WriteLine($"public static string {sourceName} => {sourceName}_field ??= global::System.Text.Encoding.UTF8.GetString({sourceName}_span);");
				writer.WriteLineNoTabs();
			}

			// Names list
			writer.WriteLine("public static global::System.Collections.Generic.IReadOnlyList<string> Names { get; } =");
			writer.WriteLine('[');
			using (new Indented(writer))
			{
				foreach ((string name, _) in sourceNames)
				{
					writer.WriteLine($"{SymbolDisplay.FormatLiteral(name, true)},");
				}
			}
			writer.WriteLine("];");
			writer.WriteLineNoTabs();

			// Get method
			writer.WriteLine("public static string? TryLoad(string name) => name switch");
			using (new CurlyBracketsWithSemicolon(writer))
			{
				foreach ((string name, string sourceName) in sourceNames)
				{
					writer.WriteLine($"{SymbolDisplay.FormatLiteral(name, true)} => {sourceName},");
				}
				writer.WriteLine($"_ => null,");
			}
		}

		context.AddSource("Licenses.g.cs", stringWriter.ToString());
	}
}
