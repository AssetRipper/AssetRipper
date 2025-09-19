using AssetRipper.Export.Configuration;
using AssetRipper.GUI.Web.Pages.Settings.DropDown;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Primitives;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Settings;

public sealed partial class SettingsPage : DefaultPage
{
	public static SettingsPage Instance { get; } = new();

	private static FullConfiguration Configuration => GameFileLoader.Settings;

	public override string GetTitle() => Localization.Settings;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).WithClass("text-center").Close(Localization.ConfigOptions);
		if (GameFileLoader.IsLoaded)
		{
			using (new Div(writer).WithClass("text-center").End())
			{
				new P(writer).Close(Localization.SettingsCanOnlyBeChangedBeforeLoadingFiles);
			}
		}
		else
		{
			using (new Form(writer).WithAction("/Settings/Update").WithMethod("post").End())
			{
				using (new Div(writer).WithClass("form-group").End())
				{
					using (new Div(writer).WithClass("border rounded p-3 m-2").End())
					{
						new H2(writer).Close(Localization.MenuImport);

						using (new Div(writer).End())
						{
							using (new Div(writer).WithClass("row").End())
							{
								using (new Div(writer).WithClass("col").End())
								{
									WriteCheckBoxForIgnoreStreamingAssets(writer, Localization.SkipStreamingAssets);
								}
								using (new Div(writer).WithClass("col").End())
								{
									WriteCheckBoxForEnableStaticMeshSeparation(writer, Localization.EnableStaticMeshSeparation, !GameFileLoader.Premium);
								}
							}

							using (new Div(writer).WithClass("row").End())
							{
								using (new Div(writer).WithClass("col").End())
								{
									WriteCheckBoxForRemoveNullableAttributes(writer, Localization.RemoveNullableAttributes);
								}
								using (new Div(writer).WithClass("col").End())
								{
									WriteCheckBoxForPublicizeAssemblies(writer, Localization.PublicizeAssemblies);
								}
							}

							using (new Div(writer).WithClass("row").End())
							{
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

						new Hr(writer).Close();

						using (new Div(writer).End())
						{
							new H3(writer).Close(Localization.Experimental);

							using (new Div(writer).WithClass("row").End())
							{
								using (new Div(writer).WithClass("col").End())
								{
									WriteCheckBoxForEnablePrefabOutlining(writer, Localization.EnablePrefabOutlining);
								}
								using (new Div(writer).WithClass("col").End())
								{
									WriteCheckBoxForEnableAssetDeduplication(writer, Localization.EnableAssetDeduplication, !GameFileLoader.Premium);
								}
							}

							using (new Div(writer).WithClass("row").End())
							{
								using (new Div(writer).WithClass("col").End())
								{
									WriteTextAreaForTargetVersion(writer);
								}
							}
						}
					}

					using (new Div(writer).WithClass("border rounded p-3 m-2").End())
					{
						new H2(writer).Close(Localization.MenuExport);

						using (new Div(writer).End())
						{
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
									WriteDropDownForLightmapTextureExportFormat(writer);
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
									WriteDropDownForShaderExportMode(writer);
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
									WriteDropDownForScriptLanguageVersion(writer);
								}
								using (new Div(writer).WithClass("col").End())
								{
									WriteDropDownForScriptExportMode(writer);
								}
								using (new Div(writer).WithClass("col").End())
								{
								}
							}

							using (new Div(writer).WithClass("row").End())
							{
								using (new Div(writer).WithClass("col").End())
								{
									WriteCheckBoxForScriptTypesFullyQualified(writer, Localization.ScriptsUseFullyQualifiedTypeNames);
								}
								using (new Div(writer).WithClass("col").End())
								{
									WriteCheckBoxForSaveSettingsToDisk(writer, Localization.SaveSettingsToDisk);
								}
								using (new Div(writer).WithClass("col").End())
								{
									WriteCheckBoxForExportUnreadableAssets(writer, Localization.ExportUnreadableAssets);
								}
							}
						}
					}

					using (new Div(writer).WithClass("text-center").End())
					{
						new Input(writer).WithType("submit").WithValue(Localization.Save).Close();
					}
				}
			}
		}
	}

	private static void WriteTextAreaForDefaultVersion(TextWriter writer)
	{
		new Label(writer).WithClass("form-label").WithFor(nameof(Configuration.ImportSettings.DefaultVersion)).Close(Localization.DefaultVersion);
		new Input(writer)
			.WithType("text")
			.WithClass("form-control")
			.WithId(nameof(Configuration.ImportSettings.DefaultVersion))
			.WithName(nameof(Configuration.ImportSettings.DefaultVersion))
			.WithValue(Configuration.ImportSettings.DefaultVersion.ToString())
			.Close();
	}

	private static void WriteTextAreaForTargetVersion(TextWriter writer)
	{
		new Label(writer).WithClass("form-label").WithFor(nameof(Configuration.ImportSettings.TargetVersion)).Close(Localization.TargetVersionForVersionChanging);
		new Input(writer)
			.WithType("text")
			.WithClass("form-control")
			.WithId(nameof(Configuration.ImportSettings.TargetVersion))
			.WithName(nameof(Configuration.ImportSettings.TargetVersion))
			.WithValue(Configuration.ImportSettings.TargetVersion.ToString())
			.Close();
	}

	private static void WriteCheckBox(TextWriter writer, string label, bool @checked, string id, bool disabled = false)
	{
		using (new Div(writer).WithClass("form-check").End())
		{
			new Input(writer)
				.WithClass("form-check-input")
				.WithType("checkbox")
				.WithValue()
				.WithId(id)
				.WithName(id)
				.MaybeWithChecked(disabled ? false : @checked)
				.MaybeWithDisabled(disabled)
				.Close();
			new Label(writer)
				.WithClass("form-check-label" + (disabled ? " text-muted" : ""))
				.WithFor(id)
				.Close(label + (disabled ? $" ({Localization.PremiumFeatureNotice})" : ""));
		}
	}

	private static void WriteDropDown<T>(TextWriter writer, DropDownSetting<T> setting, T value, string id) where T : struct, Enum
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
					.MaybeWithSelected(EqualityComparer<T>.Default.Equals(item.Value, value))
					.WithCustomAttribute("option-description", CreateUniqueID(id, i))
					.Close(item.DisplayName);
			}
		}

		for (int i = 0; i < items.Count; i++)
		{
			DropDownItem<T> item = items[i];
			new P(writer)
				.WithClass("dropdown-description")//Used for CSS selecting
				.WithId(CreateUniqueID(id, i))
				.Close(item.Description);
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

		if (Configuration.SaveSettingsToDisk)
		{
			Configuration.SaveToDefaultPath();
		}
		else
		{
			SerializedSettings.DeleteDefaultPath();
		}

		context.Response.Redirect("/Settings/Edit");
		return Task.CompletedTask;
	}
}
