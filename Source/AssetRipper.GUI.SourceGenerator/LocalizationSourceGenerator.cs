using AssetRipper.Text.SourceGeneration;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AssetRipper.GUI.SourceGenerator;

public static class LocalizationSourceGenerator
{
	private static readonly Regex SortOrderRegex = new("\\(Sort Order=([A-Z]+)\\)");
	
	public static string MakeLocalizationClass(string repositoryPath)
	{
		using StringWriter stringWriter = new();
		stringWriter.NewLine = "\n";
		using IndentedTextWriter writer = new IndentedTextWriter(stringWriter, "\t");

		writer.WriteGeneratedCodeWarning();
		writer.WriteLine();

		writer.WriteFileScopedNamespace("AssetRipper.GUI.Localizations");
		writer.WriteLine("partial class LocalizationLoader");
		using (new CurlyBrackets(writer))
		{
			AddLocalizationDictionary(writer, repositoryPath);
		}

		writer.Flush();
		
		return stringWriter.ToString();
	}
	
	private static void AddLocalizationDictionary(IndentedTextWriter writer, string repositoryPath)
	{
		string[] localizationFiles = Directory.GetFiles(Path.Combine(repositoryPath, "Localizations"), "*.json");

		writer.WriteSummaryDocumentation("Dictionary of Language codes to Language names");
		writer.WriteLine("public static IReadOnlyDictionary<string, string> LanguageNameDictionary { get; } = new Dictionary<string, string>");
		
		using (new CurlyBracketsWithSemicolon(writer))
		{
			foreach (string file in localizationFiles)
			{
				string languageCode = Path.GetFileNameWithoutExtension(file);
				string languageName = ExtractCultureName(new CultureInfo(languageCode.Replace('_', '-')));
				writer.WriteLine($"{{ \"{languageCode}\", \"{languageName}\" }},");
			}
		}
	}

	private static string ExtractCultureName(CultureInfo culture)
	{
		return SortOrderRegex.Replace(culture.NativeName, match => $"({match.Groups[1].Value})");
	}
}
