using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Assets;

public sealed class ViewPage : DefaultPage
{
	public required IUnityObjectBase Asset { get; init; }
	public required AssetPath Path { get; init; }

	public override string GetTitle() => Asset.GetBestName();

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());

		ReadOnlySpan<AssetTab> tabs =
			[
				new InformationTab(Asset, Path),
				new AudioTab(Asset),
				new ImageTab(Asset),
				new TextTab(Asset),
				new FontTab(Asset),
				new YamlTab(Asset),
				new JsonTab(Asset),
				new HexTab(Asset),
				new DependenciesTab(Asset),
				new DevelopmentTab(Asset),
			];

		using (new Nav(writer).End())
		{
			using (new Div(writer).WithClass("nav nav-tabs").WithId("nav-tab").WithRole("tablist").End())
			{
				for (int i = 0; i < tabs.Length; i++)
				{
					AssetTab tab = tabs[i];
					Button button = new Button(writer)
						.WithClass(GetTabClassSet(tab, i))
						.WithId($"nav-{tab.HtmlName}-tab")
						.WithCustomAttribute("data-bs-toggle", "tab")
						.WithCustomAttribute("data-bs-target", $"#nav-{tab.HtmlName}")
						.WithType("button")
						.WithRole("tab")
						.WithCustomAttribute("aria-controls", $"nav-{tab.HtmlName}")
						.WithCustomAttribute("aria-selected", "true");

					if (!tab.Enabled)
					{
						button.WithCustomAttribute("aria-disabled", "true");
					}

					button.Close(tab.DisplayName);
				}
			}
		}

		using (new Div(writer).WithClass("tab-content").WithId("nav-tabContent").End())
		{
			for (int i = 0; i < tabs.Length; i++)
			{
				AssetTab tab = tabs[i];
				using (new Div(writer)
					.WithClass(i == 0 && tab.Enabled ? "tab-pane fade show active" : "tab-pane fade")
					.WithId($"nav-{tab.HtmlName}")
					.WithRole("tabpanel")
					.WithCustomAttribute("aria-labelledby", $"nav-{tab.HtmlName}-tab")
					.End())
				{
					if (tab.Enabled)
					{
						tab.Write(writer);
					}
				}
			}
		}

		static string GetTabClassSet(AssetTab tab, int index)
		{
			const string DefaultTabClassSet = "nav-link";
			if (tab.Enabled)
			{
				return index == 0 ? DefaultTabClassSet + " active" : DefaultTabClassSet;
			}
			return DefaultTabClassSet + " disabled";
		}
	}

	public static Task HandlePostRequest(HttpContext context)
	{
		string? json = context.Request.Form[PathLinking.FormKey];
		if (string.IsNullOrEmpty(json))
		{
			return context.Response.NotFound("The path must be included in the request.");
		}

		AssetPath path;
		try
		{
			path = AssetPath.FromJson(json);
		}
		catch (Exception ex)
		{
			return context.Response.NotFound(ex.ToString());
		}

		if (!GameFileLoader.IsLoaded)
		{
			return context.Response.NotFound("No files loaded.");
		}
		else if (!GameFileLoader.GameBundle.TryGetAsset(path, out IUnityObjectBase? asset))
		{
			return context.Response.NotFound($"Asset could not be resolved: {path}");
		}
		else
		{
			return new ViewPage() { Asset = asset, Path = path }.WriteToResponse(context.Response);
		}
	}
}
