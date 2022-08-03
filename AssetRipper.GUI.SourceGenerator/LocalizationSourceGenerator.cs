using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace AssetRipper.GUI.SourceGenerator;

public static class LocalizationSourceGenerator
{
	private static readonly Regex SortOrderRegex = new("\\(Sort Order=([A-Z]+)\\)");
	
	public static string MakeLocalizationClass(string repositoryPath)
	{
		using StringWriter stringWriter = new();
		using IndentedTextWriter writer = new IndentedTextWriter(stringWriter);

		writer.WriteLine("// Auto-generated code");
		writer.WriteLine("using System.Collections.Generic;");
		writer.WriteLine();
		writer.WriteLine("namespace AssetRipper.GUI");
		writer.WriteLine("{");
		writer.Indent++;
		writer.WriteLine("public partial class LocalizationManager");
		writer.WriteLine("{");
		writer.Indent++;

		AddLocalizationDictionary(writer, repositoryPath);
		
		writer.Indent--;
		writer.WriteLine("}");
		writer.Indent--;
		writer.WriteLine("}");

		writer.Flush();
		
		return stringWriter.ToString();
	}
	
	private static void AddLocalizationDictionary(IndentedTextWriter writer, string repositoryPath)
	{
		string[] localizationFiles = Directory.GetFiles(Path.Combine(repositoryPath, "Localizations"), "*.json");

		writer.WriteLine("/// <summary>");
		writer.WriteLine("/// Dictionary of Language codes to Language names");
		writer.WriteLine("/// </summary>");
		writer.WriteLine("private static Dictionary<string, string> LanguageNameDictionary { get; } = new()");
		writer.WriteLine("{");
		writer.Indent++;
		
		foreach (string file in localizationFiles)
		{
			string languageCode = Path.GetFileNameWithoutExtension(file);
			string languageName = ExtractCultureName(new CultureInfo(languageCode.Replace('_', '-')));
			writer.WriteLine($"{{ \"{languageCode}\", \"{languageName}\" }},");
		}
		
		writer.Indent--;
		writer.WriteLine("};");
	}

	private static string ExtractCultureName(CultureInfo culture)
	{
		return SortOrderRegex.Replace(culture.NativeName, match => $"({match.Groups[1].Value})");
	}
}
