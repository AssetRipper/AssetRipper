using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace AssetRipper.Library.Exporters.Shaders
{
	public static class TemplateList
	{
		public const string ShaderTemplatePrefix = "AssetRipper.Library.Exporters.Shaders.Templates.";
		public const string ShaderTemplateExtension = ".txt";
		public const string TemplatesJsonPath = ShaderTemplatePrefix + "Templates.json";
		public static List<TemplateShader> Templates { get; }

		static TemplateList()
		{
			Templates = LoadTemplates();
		}

		public static TemplateShader GetBestTemplate(IShader shader)
		{
			return Templates.Where(tmp => tmp.IsMatch(shader)).MaxBy(matchedTmp => matchedTmp.RequiredProperties.Count);
		}

		private static List<TemplateShader> LoadTemplates()
		{
			Logger.Verbose("Loading shader templates");
			string jsonText = GetTextFromResource(TemplatesJsonPath);

			List<TemplateShader> templates = JsonSerializer.Deserialize<TemplateJson>(jsonText).Templates;
			foreach (TemplateShader template in templates)
			{
				string path = ShaderTemplatePrefix + template.TemplateName + ShaderTemplateExtension;
				template.ShaderText = GetTextFromResource(path);
			}
			return templates;
		}

		private static string GetTextFromResource(string path)
		{
			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
			using StreamReader reader = new(stream);
			return reader.ReadToEnd().Replace("\r", "");
		}
	}
}
