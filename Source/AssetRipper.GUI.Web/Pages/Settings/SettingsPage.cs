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
		new H1(writer).Close(Localization.ConfigOptions);
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
				WriteCheckBoxForEnablePrefabOutlining(writer, Localization.EnablePrefabOutlining);
				WriteCheckBoxForIgnoreStreamingAssets(writer, Localization.SkipStreamingAssets);
				WriteCheckBoxForIgnoreEngineAssets(writer, Localization.IgnoreEngineAssets);
				WriteTextAreaForDefaultVersion(writer);
				WriteDropDownForAudioExportFormat(writer);
				WriteDropDownForBundledAssetsExportMode(writer);
				WriteDropDownForImageExportFormat(writer);
				WriteDropDownForMeshExportFormat(writer);
				WriteDropDownForSpriteExportMode(writer);
				WriteDropDownForTerrainExportMode(writer);
				WriteDropDownForTextExportMode(writer);
				WriteDropDownForShaderExportMode(writer);
				WriteDropDownForScriptExportMode(writer);
				WriteDropDownForScriptContentLevel(writer);
				WriteDropDownForScriptLanguageVersion(writer);
				using (new Div(writer).WithClass("form-group").End())
				{
					new Input(writer).WithClass("btn btn-primary").WithType("submit").WithValue(Localization.Save).Close();
				}
			}
		}
	}

	private static void WriteTextAreaForDefaultVersion(TextWriter writer)
	{
		new Label(writer).WithClass("form-label").WithFor(nameof(Configuration.DefaultVersion)).Close(Localization.DefaultVersion);
		new Input(writer)
			.WithType("text")
			.WithClass("form-control")
			.WithId(nameof(Configuration.DefaultVersion))
			.WithName(nameof(Configuration.DefaultVersion))
			.WithValue(Configuration.DefaultVersion.ToString())
			.Close();
	}

	private static void WriteCheckBox(TextWriter writer, string label, bool @checked, string id)
	{
		using (new Div(writer).WithClass("form-check").End())
		{
			new Input(writer).WithClass("form-check-input").WithType("checkbox").WithValue().WithId(id).WithName(id).MaybeWithChecked(@checked).Close();
			new Label(writer).WithClass("form-check-label").WithFor(id).Close(label);
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
