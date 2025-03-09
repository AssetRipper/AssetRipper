using AssetRipper.Text.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SGF;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace AssetRipper.GUI.Localizations.SourceGenerator;

[IncrementalGenerator]
public sealed class LocalizationGenerator : IncrementalGenerator
{
	public LocalizationGenerator() : base(typeof(LocalizationGenerator).FullName)
	{
	}

	public override void OnInitialize(SgfInitializationContext context)
	{
		IncrementalValueProvider<ImmutableArray<(string name, string? json)>> pipeline = context.AdditionalTextsProvider
			.Where(static (text) => text.Path.EndsWith(".json"))
			.Select(static (text, cancellationToken) =>
			{
				string name = Path.GetFileName(text.Path);
				string? json = text.GetText(cancellationToken)?.ToString();
				return (name, json);
			})
			.Where(static (pair) => pair.json != null)
			.Collect();

		context.RegisterSourceOutput(pipeline, GenerateCode);
	}

	private static void GenerateCode(SgfSourceProductionContext context, ImmutableArray<(string, string?)> files)
	{
		List<(string, Dictionary<string, string>)> list = new(files.Length);
		foreach ((string file, string? json) in files.OrderBy(s => s.Item1))
		{
			string languageCode = Path.GetFileNameWithoutExtension(file).Replace('_', '-');
			Dictionary<string, string> dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json!) ?? throw new();
			list.Add((languageCode, dictionary));
		}
		list = list.OrderBy(pair => pair.Item1).ToList();

		Dictionary<string, string> americanEnglish = list.First(pair => pair.Item1 == "en-US").Item2;

		StringWriter stringWriter = new();
		IndentedTextWriter writer = IndentedTextWriterFactory.Create(stringWriter);

		writer.WriteGeneratedCodeWarning();
		writer.WriteLineNoTabs();

		writer.WriteFileScopedNamespace("AssetRipper.GUI.Localizations");
		writer.WriteLine("partial class Localization");
		using (new CurlyBrackets(writer))
		{
			foreach (KeyValuePair<string, string> pair in americanEnglish.OrderBy(pair => pair.Key))
			{
				(string key, string content) = (pair.Key, pair.Value);
				writer.WriteSummaryDocumentation(content);
				writer.WriteLine($"public static string {SnakeCaseToPascalCase(key)} => CurrentLanguageCode switch");
				using (new CurlyBracketsWithSemicolon(writer))
				{
					foreach ((string languageCode, Dictionary<string, string> dictionary) in list)
					{
						if (languageCode == "en-US" || !dictionary.TryGetValue(key, out string? value))
						{
							continue;
						}
						writer.WriteLine($"\"{languageCode}\" => {SymbolDisplay.FormatLiteral(value, quote: true)},");
					}
					writer.WriteLine($"_ => {SymbolDisplay.FormatLiteral(content, quote: true)},");
				}
				writer.WriteLineNoTabs();
			}
		}

		writer.Flush();

		context.AddSource("Localization.g.cs", stringWriter.ToString());
	}

	private static string SnakeCaseToPascalCase(string str)
	{
		StringBuilder builder = new(str.Length);
		bool nextUpper = true;
		foreach (char c in str)
		{
			if (c == '_')
			{
				nextUpper = true;
			}
			else if (nextUpper)
			{
				builder.Append(char.ToUpperInvariant(c));
				nextUpper = false;
			}
			else
			{
				builder.Append(c);
			}
		}
		return builder.ToString();
	}
}
