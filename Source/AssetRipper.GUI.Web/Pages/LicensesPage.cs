using AssetRipper.GUI.Licensing;

namespace AssetRipper.GUI.Web.Pages;

public sealed class LicensesPage : DefaultPage
{
	public static LicensesPage Instance { get; } = new();

	private readonly HtmlTab[] tabs = Licenses.Names.Select(name => new LicenseTab(name)).ToArray();

	public override string? GetTitle() => Localization.Licenses;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());
		using (new Div(writer).WithClass("container text-center").End())
		{
			using (new Div(writer).WithClass("row").End())
			{
				using (new Div(writer).WithClass("col-3").End())
				{
					HtmlTab.WriteNavigation(writer, tabs, "nav nav-pills flex-column");
				}
				using (new Div(writer).WithClass("col-9").End())
				{
					HtmlTab.WriteContent(writer, tabs);
				}
			}
		}
	}

	private sealed class LicenseTab(string name) : HtmlTab
	{
		public override string DisplayName => name;
		public override void Write(TextWriter writer)
		{
			new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").Close(Licenses.Load(name));
		}
	}
}
