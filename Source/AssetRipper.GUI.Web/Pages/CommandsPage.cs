using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages;

public sealed class CommandsPage : VuePage
{
	public static CommandsPage Instance { get; } = new();

	public override string? GetTitle() => Localization.Commands;

	public override void WriteInnerContent(TextWriter writer)
	{
		if (!GameFileLoader.IsLoaded)
		{
			using (new P(writer).End())
			{
				using (new Form(writer).WithAction("/LoadFolder").WithMethod("post").End())
				{
					new Input(writer).WithClass("form-control").WithType("text").WithName("Path")
						.WithCustomAttribute("v-model", "load_path")
						.WithCustomAttribute("@input", "handleLoadPathChange").Close();
					new Input(writer).WithCustomAttribute("v-if", "load_path_exists").WithType("submit").WithClass("btn btn-primary").WithValue(Localization.MenuLoad).Close();
					new Button(writer).WithCustomAttribute("v-else").WithClass("btn btn-primary").WithCustomAttribute("disabled").Close(Localization.MenuLoad);
				}

				if (Dialogs.Supported)
				{
					new Button(writer).WithCustomAttribute("@click", "handleSelectLoadFile").WithClass("btn btn-success").Close(Localization.SelectFile);
					new Button(writer).WithCustomAttribute("@click", "handleSelectLoadFolder").WithClass("btn btn-success").Close(Localization.SelectFolder);
				}
			}
		}
		else
		{
			using (new P(writer).End())
			{
				WriteLink(writer, "/Reset", Localization.MenuFileReset, "btn btn-danger");
			}
			using (new P(writer).End())
			{
				using (new Form(writer).End())
				{
					new Input(writer).WithClass("form-control").WithType("text").WithName("Path")
						.WithCustomAttribute("v-model", "export_path")
						.WithCustomAttribute("@input", "handleExportPathChange").Close();
				}

				using (new Div(writer).WithClass("form-check mb-2").End())
				{
					new Input(writer)
						.WithType("checkbox")
						.WithClass("form-check-input")
						.WithCustomAttribute("v-model", "create_subfolder")
						.WithCustomAttribute("@input", "handleExportPathChange")
						.WithId("createSubfolder")
						.Close();
					new Label(writer)
						.WithClass("form-check-label")
						.WithCustomAttribute("for", "createSubfolder")
						.Close(Localization.CreateSubfolder);
				}

				using (new Form(writer).WithAction("/Export/UnityProject").WithMethod("post").End())
				{
					new Input(writer).WithType("hidden").WithName("Path").WithCustomAttribute("v-model", "export_path").Close();
					new Input(writer).WithType("hidden").WithName("CreateSubfolder").WithCustomAttribute("v-model", "create_subfolder").Close();

					new Button(writer).WithCustomAttribute("v-if", "export_path === '' || export_path !== export_path.trim()").WithClass("btn btn-primary").WithCustomAttribute("disabled").Close(Localization.ExportUnityProject);
					new Input(writer).WithCustomAttribute("v-else-if", "export_path_has_files").WithType("submit").WithClass("btn btn-danger").WithValue(Localization.ExportUnityProject).Close();
					new Input(writer).WithCustomAttribute("v-else").WithType("submit").WithClass("btn btn-primary").WithValue(Localization.ExportUnityProject).Close();
				}

				using (new Form(writer).WithAction("/Export/PrimaryContent").WithMethod("post").End())
				{
					new Input(writer).WithType("hidden").WithName("Path").WithCustomAttribute("v-model", "export_path").Close();
					new Input(writer).WithType("hidden").WithName("CreateSubfolder").WithCustomAttribute("v-model", "create_subfolder").Close();

					new Button(writer).WithCustomAttribute("v-if", "export_path === '' || export_path !== export_path.trim()").WithClass("btn btn-primary").WithCustomAttribute("disabled").Close(Localization.ExportPrimaryContent);
					new Input(writer).WithCustomAttribute("v-else-if", "export_path_has_files").WithType("submit").WithClass("btn btn-danger").WithValue(Localization.ExportPrimaryContent).Close();
					new Input(writer).WithCustomAttribute("v-else").WithType("submit").WithClass("btn btn-primary").WithValue(Localization.ExportPrimaryContent).Close();
				}

				if (Dialogs.Supported)
				{
					new Button(writer).WithCustomAttribute("@click", "handleSelectExportFolder").WithClass("btn btn-success").Close(Localization.SelectFolder);
				}

				using (new Div(writer).WithCustomAttribute("v-if", "export_path_has_files").End())
				{
					new P(writer).Close(Localization.WarningThisDirectoryIsNotEmptyAllContentWillBeDeleted);
				}
			}
		}
	}

	protected override void WriteScriptReferences(TextWriter writer)
	{
		base.WriteScriptReferences(writer);
		new Script(writer).WithSrc("/js/commands_page.js").Close();
	}

	private static void WriteLink(TextWriter writer, string url, string name, string? @class = null)
	{
		using (new Form(writer).WithAction(url).WithMethod("post").End())
		{
			new Input(writer).WithType("submit").WithClass(@class).WithValue(name.ToHtml()).Close();
		}
	}
}
