using AssetRipper.Text.SourceGeneration;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AssetRipper.GUI.SourceGenerator;

public static partial class LocalizationSourceGenerator
{
	[GeneratedRegex("\\(Sort Order=([A-Z]+)\\)")]
	private static partial Regex SortOrderRegex();

	public static void MakeLocalizationClass()
	{
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(Paths.LocalizationsProjectPath, "Localization");

		writer.WriteGeneratedCodeWarning();
		writer.WriteLine();

		writer.WriteFileScopedNamespace("AssetRipper.GUI.Localizations");
		writer.WriteLine("partial class Localization");
		using (new CurlyBrackets(writer))
		{
			AddLocalizationProperties(writer);
		}

		writer.Flush();
	}

	private static void AddLocalizationProperties(IndentedTextWriter writer)
	{
		Dictionary<string, string> dictionary = JsonSerializer.Deserialize(File.OpenRead(Paths.LocalizationsPath + "en_US.json"), DictionarySerializerContext.Default.DictionaryStringString)!;
		foreach (KeyValuePair<string, string> pair in dictionary)
		{
			writer.WriteSummaryDocumentation(pair.Value);
			writer.WriteLine($"public static string {SnakeCaseToPascalCase(pair.Key)} => Get(\"{pair.Key}\");");
			writer.WriteLine();
		}

		static string SnakeCaseToPascalCase(string str)
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

	public static void MakeLocalizationLoaderClass()
	{
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(Paths.LocalizationsProjectPath, "LocalizationLoader");

		writer.WriteGeneratedCodeWarning();
		writer.WriteLine();

		writer.WriteFileScopedNamespace("AssetRipper.GUI.Localizations");
		writer.WriteLine("partial class LocalizationLoader");
		using (new CurlyBrackets(writer))
		{
			AddLocalizationDictionary(writer);
			writer.WriteLineNoTabs();
			AddUnderscoredAndHyphenatedConversion(writer);
		}

		writer.Flush();
	}

	private static void AddLocalizationDictionary(IndentedTextWriter writer)
	{
		string[] localizationFiles = Directory.GetFiles(Paths.LocalizationsPath, "*.json");

		writer.WriteSummaryDocumentation("Dictionary of Language codes to Language names");
		writer.WriteLine("public static IReadOnlyDictionary<string, string> LanguageNameDictionary { get; } = new Dictionary<string, string>");

		using (new CurlyBracketsWithSemicolon(writer))
		{
			foreach (string file in localizationFiles)
			{
				string languageCode = Path.GetFileNameWithoutExtension(file).Replace('_', '-');
				string languageName = ExtractCultureName(new CultureInfo(languageCode));
				writer.WriteLine($"{{ \"{languageCode}\", \"{languageName}\" }},");
			}
		}
	}

	private static void AddUnderscoredAndHyphenatedConversion(IndentedTextWriter writer)
	{
		string[] localizationFiles = Directory.GetFiles(Paths.LocalizationsPath, "*.json");

		writer.WriteSummaryDocumentation("Convert a language code to its underscored variant.");
		writer.WriteRemarksDocumentation("This is used by the json file names.");
		writer.WriteLine("internal static string AsUnderscoredLanguageCode(string value) => value switch");
		using (new CurlyBracketsWithSemicolon(writer))
		{
			foreach (string file in localizationFiles)
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
		writer.WriteLine("internal static string AsHyphenatedLanguageCode(string value) => value switch");
		using (new CurlyBracketsWithSemicolon(writer))
		{
			foreach (string file in localizationFiles)
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
		return SortOrderRegex().Replace(culture.NativeName, match => $"({match.Groups[1].Value})");
	}
}
