using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Text.SourceGeneration;
using System.CodeDom.Compiler;
using System.Diagnostics;

namespace AssetRipper.GUI.Web.SourceGenerator;

internal static class Program
{
	static void Main()
	{
		GenerateHelperClass(IndentedTextWriterFactory.Create("../../../AssetRipper.GUI.Web/Pages/Settings", "SettingsPage"));
		Console.WriteLine("Done!");
	}

	static void GenerateHelperClass(IndentedTextWriter writer)
	{
		writer.WriteGeneratedCodeWarning();
		writer.WriteLineNoTabs();
		writer.WriteUsing("AssetRipper.Export.UnityProjects.Configuration");
		writer.WriteUsing("AssetRipper.GUI.Web.Pages.Settings.DropDown");
		writer.WriteUsing("AssetRipper.Import.Configuration");
		writer.WriteLineNoTabs();
		writer.WriteFileScopedNamespace("AssetRipper.GUI.Web.Pages.Settings");
		writer.WriteLineNoTabs();
		writer.WriteLine("#nullable enable");
		writer.WriteLineNoTabs();
		writer.WriteLine("partial class SettingsPage");
		using (new CurlyBrackets(writer))
		{
			List<PropertyData> properties = GetPropertyNames().Select(name => new PropertyData(name)).ToList();
			writer.WriteLine("private static void SetProperty(string key, string? value)");
			using (new CurlyBrackets(writer))
			{
				writer.WriteLine("switch (key)");
				using (new CurlyBrackets(writer))
				{
					foreach (PropertyData property in properties.Where(p => !p.IsBoolean))
					{
						writer.WriteLine($"case {property.NameOfString}:");
						using (new Indented(writer))
						{
							writer.Write("Configuration.");
							writer.Write(property.Name);
							writer.Write(" = ");
							if (property.IsUnityVersion)
							{
								writer.WriteLine("TryParseUnityVersion(value);");
							}
							else if (property.IsEnum)
							{
								writer.Write("TryParseEnum<");
								writer.Write(property.PropertyType.Name);
								writer.WriteLine(">(value);");
							}
							else
							{
								Debug.Assert(property.IsString);
								writer.WriteLine("value;");
							}
							writer.WriteLine("break;");
						}
					}
				}
			}

			writer.WriteLineNoTabs();
			writer.WriteLine("private static readonly Dictionary<string, Action<bool>> booleanProperties = new()");
			using (new CurlyBracketsWithSemicolon(writer))
			{
				foreach (PropertyData property in properties)
				{
					if (property.IsBoolean)
					{
						writer.WriteLine($"{{ {property.NameOfString}, (value) => {{ Configuration.{property.Name} = value; }} }},");
					}
				}
			}

			foreach (PropertyData property in properties)
			{
				if (property.IsBoolean)
				{
					writer.WriteLineNoTabs();
					writer.WriteLine($"private static void WriteCheckBoxFor{property.Name}(TextWriter writer, string label)");
					using (new CurlyBrackets(writer))
					{
						writer.WriteLine($"WriteCheckBox(writer, label, Configuration.{property.Name}, {property.NameOfString});");
					}
				}
				else if (property.IsEnum)
				{
					writer.WriteLineNoTabs();
					writer.WriteLine($"private static void WriteDropDownFor{property.Name}(TextWriter writer)");
					using (new CurlyBrackets(writer))
					{
						writer.WriteLine($"WriteDropDown(writer, {property.PropertyType.Name}DropDownSetting.Instance, Configuration.{property.Name}, {property.NameOfString});");
					}
				}
			}
		}
	}

	static IEnumerable<string> GetPropertyNames()
	{
		return
			[
				nameof(LibraryConfiguration.EnablePrefabOutlining),
				nameof(LibraryConfiguration.IgnoreStreamingAssets),
				nameof(LibraryConfiguration.IgnoreEngineAssets),
				nameof(LibraryConfiguration.DefaultVersion),
				nameof(LibraryConfiguration.AudioExportFormat),
				nameof(LibraryConfiguration.BundledAssetsExportMode),
				nameof(LibraryConfiguration.ImageExportFormat),
				nameof(LibraryConfiguration.MeshExportFormat),
				nameof(LibraryConfiguration.SpriteExportMode),
				nameof(LibraryConfiguration.TerrainExportMode),
				nameof(LibraryConfiguration.TextExportMode),
				nameof(LibraryConfiguration.ShaderExportMode),
				nameof(LibraryConfiguration.ScriptExportMode),
				nameof(LibraryConfiguration.ScriptContentLevel),
				nameof(LibraryConfiguration.ScriptLanguageVersion),
			];
	}
}
