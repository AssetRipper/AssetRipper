using AssetRipper.Export.Configuration;
using AssetRipper.Import.Configuration;
using AssetRipper.Processing.Configuration;
using AssetRipper.Text.SourceGeneration;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;

namespace AssetRipper.GUI.SourceGenerator;

internal static class SettingsPageGenerator
{
	public static void Run()
	{
		GenerateHelperClass(IndentedTextWriterFactory.Create(Paths.WebProjectPath + "Pages/Settings", "SettingsPage"));
		Console.WriteLine("Done!");
	}

	static void GenerateHelperClass(IndentedTextWriter writer)
	{
		writer.WriteGeneratedCodeWarning();
		writer.WriteLineNoTabs();
		writer.WriteUsing("AssetRipper.Export.Configuration");
		writer.WriteUsing("AssetRipper.GUI.Web.Pages.Settings.DropDown");
		writer.WriteUsing("AssetRipper.Import.Configuration");
		writer.WriteUsing("AssetRipper.Processing.Configuration");
		writer.WriteLineNoTabs();
		writer.WriteFileScopedNamespace("AssetRipper.GUI.Web.Pages.Settings");
		writer.WriteLineNoTabs();
		writer.WriteLine("#nullable enable");
		writer.WriteLineNoTabs();
		writer.WriteLine("partial class SettingsPage");
		using (new CurlyBrackets(writer))
		{
			List<PropertyData> properties = GetProperties().ToList();
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
							writer.Write($"Configuration.{property.DeclaringType?.Name}.");
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
						writer.WriteLine($"{{ {property.NameOfString}, (value) => {{ Configuration.{property.DeclaringType?.Name}.{property.Name} = value; }} }},");
					}
				}
			}

			foreach (PropertyData property in properties)
			{
				if (property.IsBoolean)
				{
					writer.WriteLineNoTabs();
					writer.WriteLine($"private static void WriteCheckBoxFor{property.Name}(TextWriter writer, string label, bool disabled = false)");
					using (new CurlyBrackets(writer))
					{
						writer.WriteLine($"WriteCheckBox(writer, label, Configuration.{property.DeclaringType?.Name}.{property.Name}, {property.NameOfString}, disabled);");
					}
				}
				else if (property.IsEnum)
				{
					writer.WriteLineNoTabs();
					writer.WriteLine($"private static void WriteDropDownFor{property.Name}(TextWriter writer)");
					using (new CurlyBrackets(writer))
					{
						writer.WriteLine($"WriteDropDown(writer, {property.PropertyType.Name}DropDownSetting.Instance, Configuration.{property.DeclaringType?.Name}.{property.Name}, {property.NameOfString});");
					}
				}
			}
		}
	}

	static IEnumerable<PropertyData> GetProperties()
	{
		foreach (Type type in (Type[])[typeof(ImportSettings), typeof(ProcessingSettings), typeof(ExportSettings)])
		{
			foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				yield return new PropertyData(property);
			}
		}
	}
}
