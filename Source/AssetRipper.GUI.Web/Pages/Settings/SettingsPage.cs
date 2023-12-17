using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.GUI.Web.Pages.Settings.DropDown;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Primitives;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Settings;

public sealed partial class SettingsPage : DefaultPage
{
	public static SettingsPage Instance { get; } = new();

	private static LibraryConfiguration Configuration => GameFileLoader.Settings;

	public override string GetTitle() => Localization.Settings;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).WithStyle("font-family: Arial, sans-serif;").Close(Localization.ConfigOptions);
		if (GameFileLoader.IsLoaded)
		{
			using (new Div(writer).WithStyle("text-align:center; font-family: Arial, sans-serif;").End())
			{
				new P(writer).WithStyle("font-family: Arial, sans-serif;")
					.Close(Localization.SettingsCanOnlyBeChangedBeforeLoadingFiles);
			}
		}
		else
		{
			using (new Div(writer).WithClass("container").End())
			{
				using (new Form(writer).WithAction("/Settings/Update").WithMethod("post").End())
				{
					using (new Div(writer)
						       .WithStyle(
							       "margin-bottom: 20px; border: 1px solid #ccc; border-radius: 5px; font-family: Arial, sans-serif;")
						       .End())
					{
						using (new Div(writer).WithStyle(
								       "padding: 10px; border-radius: 5px; font-family: Arial, sans-serif;")
							       .End())
						{
							new H2(writer).WithStyle("font-family: Arial, sans-serif;").Close("Import");
							
							using (new Div(writer).WithClass("row").End())
							{
								WriteCheckBoxForIgnoreStreamingAssets(writer,
									Localization.SkipStreamingAssets);
								
								using (new Div(writer).WithClass("col").End())
								{
									WriteTextAreaForDefaultVersion(writer);
								}
							}
							
							using (new Div(writer).WithClass("row").End())
							{
								
								using (new Div(writer).WithClass("col").End())
								{
									WriteDropDownForBundledAssetsExportMode(writer);
								}
								
								using (new Div(writer).WithClass("col").End())
								{
									WriteDropDownForScriptContentLevel(writer);
								}
							}
						}

						new Hr(writer).WithStyle("border-color: #ccc; margin: 0;").Close();

						using (new Div(writer)
							       .WithStyle(
								       "padding: 10px; font-family: Arial, sans-serif;")
							       .End())
						{
							new H3(writer).WithStyle("font-family: Arial, sans-serif;").Close("Experimental");

							WriteCheckBoxForEnablePrefabOutlining(writer, Localization.EnablePrefabOutlining);
						}
					}

					using (new Div(writer)
						       .WithStyle(
							       "margin-bottom: 20px; border: 1px solid #ccc; border-radius: 5px; font-family: Arial, sans-serif;")
						       .End())
					{
						using (new Div(writer).WithStyle(
								       "padding: 10px; border-radius: 5px; font-family: Arial, sans-serif;")
							       .End())
						{
							new H2(writer).WithStyle("font-family: Arial, sans-serif;").Close(Localization.MenuExport);
							
							using (new Div(writer).WithClass("row").End())
							{
								using (new Div(writer).WithClass("col").End())
								{
									WriteDropDownForAudioExportFormat(writer);
								}
								
								using (new Div(writer).WithClass("col").End())
								{
									WriteDropDownForImageExportFormat(writer);
								}
								
								using (new Div(writer).WithClass("col").End())
								{
									WriteDropDownForMeshExportFormat(writer);
								}
							}
							
							using (new Div(writer).WithClass("row").End())
							{
								using (new Div(writer).WithClass("col").End())
								{
									WriteDropDownForSpriteExportMode(writer);
								}
								using (new Div(writer).WithClass("col").End())
								{
									WriteDropDownForTerrainExportMode(writer);
								}
								using (new Div(writer).WithClass("col").End())
								{
									WriteDropDownForTextExportMode(writer);
								}
							}
							
							using (new Div(writer).WithClass("row").End())
							{
								using (new Div(writer).WithClass("col").End())
								{
									WriteDropDownForShaderExportMode(writer);
								}
								using (new Div(writer).WithClass("col").End())
								{
									WriteDropDownForScriptLanguageVersion(writer);
								}
								using (new Div(writer).WithClass("col").End())
								{
									WriteDropDownForScriptExportMode(writer);
								}
							}
						}

						new Hr(writer).WithStyle("border-color: #ccc; margin: 0;").Close();

						using (new Div(writer)
							       .WithStyle(
								       "padding: 10px; font-family: Arial, sans-serif;")
							       .End())
						{
							new H3(writer).WithStyle("font-family: Arial, sans-serif;").Close("Experimental");

							WriteCheckBoxForIgnoreEngineAssets(writer,
								Localization.IgnoreEngineAssets);
						}
					}

					using (new Div(writer).WithStyle("margin: 10px 0;").End())
					{
						new Input(writer)
							.WithStyle(
								"background-color: #007bff; border-color: #007bff; color: white; padding: 10px 20px; border-radius: 5px; cursor: pointer;")
							.WithType("submit").WithValue(Localization.Save).Close();
					}
				}
			}
		}
	}

	private static void WriteTextAreaForDefaultVersion(TextWriter writer)
	{
		new Label(writer).WithStyle("margin-top: 10px;").WithFor(nameof(Configuration.DefaultVersion))
			.Close(Localization.DefaultVersion);
		new Input(writer)
			.WithType("text")
			.WithStyle("padding: 10px; border: 1px solid #ccc; border-radius: 5px; width: 100%; margin-Bottom: 10px;")
			.WithId(nameof(Configuration.DefaultVersion))
			.WithName(nameof(Configuration.DefaultVersion))
			.WithValue(Configuration.DefaultVersion.ToString())
			.Close();
	}

	private static void WriteCheckBox(TextWriter writer, string label, bool @checked, string id)
	{
		using (new Div(writer).WithStyle("margin-bottom: 10px;").End())
		{
			new Input(writer).WithStyle("margin-right: 10px;").WithType("checkbox").WithValue().WithId(id).WithName(id)
				.MaybeWithChecked(@checked).Close();
			new Label(writer).WithFor(id).Close(label);
		}
	}

	private static void WriteDropDown<T>(TextWriter writer, DropDownSetting<T> setting, T value, string id)
		where T : struct, Enum
	{
		IReadOnlyList<DropDownItem<T>> items = setting.GetValues();
		new Label(writer).WithClass("form-label").WithFor(id).Close(setting.Title);
		using (new Select(writer).WithClass("form-select").WithName(id).End())
		{
			for (int i = 0; i < items.Count; i++)
			{
				DropDownItem<T> item = items[i];
				new Option(writer)
					.WithValue(item.Value.ToString().ToHtml())
					.MaybeSelected(EqualityComparer<T>.Default.Equals(item.Value, value))
					.WithCustomAttribute("option-description", CreateUniqueID(id, i))
					.Close(item.DisplayName);
			}
		}

		for (int i = 0; i < items.Count; i++)
		{
			DropDownItem<T> item = items[i];
			new P(writer).WithId(CreateUniqueID(id, i)).Close(item.Description);
		}

		static string CreateUniqueID(string selectID, int index)
		{
			return $"{selectID}_description_{index}";
		}
	}

	private static UnityVersion TryParseUnityVersion(string? version)
	{
		if (string.IsNullOrEmpty(version))
		{
			return default;
		}

		try
		{
			return UnityVersion.Parse(version);
		}
		catch
		{
			return default;
		}
	}

	private static T TryParseEnum<T>(string? s) where T : struct, Enum
	{
		if (Enum.TryParse(s, out T result))
		{
			return result;
		}

		return default;
	}

	public static Task HandlePostRequest(HttpContext context)
	{
		IFormCollection form = context.Request.Form;
		foreach ((string key, Action<bool> action) in booleanProperties)
		{
			action.Invoke(form.ContainsKey(key));
		}

		foreach ((string key, string? value) in form.Select(pair => (pair.Key, (string?)pair.Value)))
		{
			SetProperty(key, value);
		}

		context.Response.Redirect("/Settings/Edit");
		return Task.CompletedTask;
	}
}
