using AssetRipper.GUI.Licensing;
using System.Net;

namespace AssetRipper.GUI.Web.Pages;

public sealed class LicensesPage : DefaultPage
{
	public static LicensesPage Instance { get; } = new();

	public override string? GetTitle() => Localization.Licenses;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());
		using (new Div(writer).WithClass("container text-center").End())
		{
			using (new Div(writer).WithClass("row").End())
			{
				using (new Div(writer).WithClass("col-4").End())
				{
					using (new Nav(writer).End())
					{
						using (new Div(writer).WithClass("nav nav-pills flex-column").WithId("nav-tab").WithRole("tablist").End())
						{
							foreach (string name in Licenses.Names)
							{
								string id = $"nav-{name}-tab";
								string target = $"#nav-{name}";
								new Button(writer)
									.WithClass("nav-link")
									.WithId(id)
									.WithCustomAttribute("data-bs-toggle", "tab")
									.WithCustomAttribute("data-bs-target", target)
									.WithType("button")
									.WithRole("tab")
									.WithCustomAttribute("aria-controls", "nav-hex")
									.WithCustomAttribute("aria-selected", "false")
									.Close(name);
							}
						}
					}
				}
				using (new Div(writer).WithClass("col-8").End())
				{
					using (new Div(writer).WithClass("tab-content").WithId("nav-tabContent").End())
					{
						foreach (string name in Licenses.Names)
						{
							string id = $"nav-{name}";
							string tab = $"#nav-{name}-tab";
							using (new Div(writer).WithClass("tab-pane fade").WithId(id).WithRole("tabpanel").WithCustomAttribute("aria-labelledby", tab).End())
							{
								new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").Close(Licenses.Load(name));
							}
						}
					}
				}
			}
		}
	}
}
