using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using System.Reflection;
using System.Text.Json;

namespace AssetRipper.Export.UnityProjects.Shaders;

public static class TemplateList
{
	public const string ShaderTemplatePrefix = "AssetRipper.Export.UnityProjects.Shaders.Templates.";
	public const string ShaderTemplateExtension = ".txt";
	public const string TemplatesJsonPath = ShaderTemplatePrefix + "Templates.json";
	public static List<TemplateShader> Templates { get; }

	static TemplateList()
	{
		Templates = LoadTemplates();
	}

	public static TemplateShader GetBestTemplate(IShader shader)
	{
		return Templates.Where(tmp => tmp.IsMatch(shader)).MaxBy(matchedTmp => matchedTmp.RequiredProperties.Count)!;
	}

	private static List<TemplateShader> LoadTemplates()
	{
		Logger.Verbose("Loading shader templates");
		string jsonText = GetTextFromResource(TemplatesJsonPath);

		TemplateJson templateJson = JsonSerializer.Deserialize(jsonText, TemplateJsonSerializerContext.Default.TemplateJson)
			?? throw new Exception("Failed to deserialize json");
		foreach (TemplateShader template in templateJson.Templates)
		{
			string path = ShaderTemplatePrefix + template.TemplateName + ShaderTemplateExtension;
			template.ShaderText = GetTextFromResource(path);
		}
		return templateJson.Templates;
	}

	private static string GetTextFromResource(string path)
	{
		Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path)
			?? throw new ArgumentException($"No stream at path: {path}", nameof(path));
		using StreamReader reader = new(stream);
		return reader.ReadToEnd().Replace("\r", "");
	}
}
