using AssetRipper.Text.SourceGeneration;
using Microsoft.CodeAnalysis;
using SGF;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AssetRipper.GUI.Localizations.SourceGenerator;

[IncrementalGenerator]
public sealed partial class LanguageCodesGenerator : IncrementalGenerator
{
	public LanguageCodesGenerator() : base(typeof(LanguageCodesGenerator).FullName)
	{
	}

	public override void OnInitialize(SgfInitializationContext context)
	{
		IncrementalValueProvider<ImmutableArray<string>> pipeline = context.AdditionalTextsProvider
			.Where(static (text) => text.Path.EndsWith(".json"))
			.Select(static (text, cancellationToken) =>
			{
				return Path.GetFileName(text.Path);
			})
			.Collect();

		context.RegisterSourceOutput(pipeline, GenerateCode);
	}

	private static void GenerateCode(SgfSourceProductionContext context, ImmutableArray<string> files)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter writer = IndentedTextWriterFactory.Create(stringWriter);

		writer.WriteGeneratedCodeWarning();
		writer.WriteLineNoTabs();

		writer.WriteLineNoTabs("#nullable enable");

		writer.WriteFileScopedNamespace("AssetRipper.GUI.Localizations");
		writer.WriteLine("public static partial class LanguageCodes");
		using (new CurlyBrackets(writer))
		{
			AddLocalizationDictionary(writer, files);
			writer.WriteLineNoTabs();
			AddUnderscoredAndHyphenatedConversion(writer, files);
		}

		writer.Flush();

		context.AddSource("LanguageCodes.g.cs", stringWriter.ToString());
	}

	private static void AddLocalizationDictionary(IndentedTextWriter writer, ImmutableArray<string> files)
	{
		writer.WriteSummaryDocumentation("Dictionary of Language codes to Language names");
		writer.WriteLine("public static IReadOnlyDictionary<string, string> LanguageNameDictionary { get; } = new Dictionary<string, string>");

		using (new CurlyBracketsWithSemicolon(writer))
		{
			foreach (string file in files.OrderBy(s => s))
			{
				string languageCode = Path.GetFileNameWithoutExtension(file).Replace('_', '-');
				string languageName = ExtractCultureName(new CultureInfo(languageCode));
				writer.WriteLine($"{{ \"{languageCode}\", \"{languageName}\" }},");
			}
		}
	}

	private static void AddUnderscoredAndHyphenatedConversion(IndentedTextWriter writer, ImmutableArray<string> files)
	{
		writer.WriteSummaryDocumentation("Convert a language code to its underscored variant.");
		writer.WriteRemarksDocumentation("This is used by the json file names.");
		writer.WriteLine("[return: global::System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(value))]");
		writer.WriteLine("internal static string? AsUnderscoredLanguageCode(string? value) => value switch");
		using (new CurlyBracketsWithSemicolon(writer))
		{
			foreach (string file in files)
			{
				string underscoredCode = Path.GetFileNameWithoutExtension(file);
				if (underscoredCode.Contains('_'))
				{
					string hyphenatedCode = underscoredCode.Replace('_', '-');
					writer.WriteLine($"\"{hyphenatedCode}\" => \"{underscoredCode}\",");
				}
			}
			writer.WriteLine("_ => value");
		}

		writer.WriteLineNoTabs();

		writer.WriteSummaryDocumentation("Convert a language code to its hyphenated variant.");
		writer.WriteRemarksDocumentation("This is used by HTML and the IANA Language Subtag Registry.");
		writer.WriteLine("[return: global::System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(value))]");
		writer.WriteLine("internal static string? AsHyphenatedLanguageCode(string? value) => value switch");
		using (new CurlyBracketsWithSemicolon(writer))
		{
			foreach (string file in files)
			{
				string underscoredCode = Path.GetFileNameWithoutExtension(file);
				if (underscoredCode.Contains('_'))
				{
					string hyphenatedCode = underscoredCode.Replace('_', '-');
					writer.WriteLine($"\"{underscoredCode}\" => \"{hyphenatedCode}\",");
				}
			}
			writer.WriteLine("_ => value");
		}
	}

	private static string ExtractCultureName(CultureInfo culture)
	{
		return SortOrderRegex.Replace(culture.NativeName, match => $"({match.Groups[1].Value})");
	}

	private static Regex SortOrderRegex { get; } = new Regex(@"\(Sort Order=([A-Z]+)\)", RegexOptions.Compiled);
}
