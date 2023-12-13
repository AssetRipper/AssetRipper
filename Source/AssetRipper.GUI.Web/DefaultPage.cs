using AssetRipper.Web.Content;

namespace AssetRipper.GUI.Web;
public abstract class DefaultPage : HtmlPage
{
	public sealed override void Write(TextWriter writer)
	{
		base.Write(writer);
		using (new Html(writer).WithLang(Localizations.Localization.CurrentLanguageCode).End())
		{
			using (new Head(writer).End())
			{
				new Meta(writer).WithCharset("utf-8").Close();
				new Meta(writer).WithName("viewport").WithContent("width=device-width, initial-scale=1.0").Close();
				new Title(writer).Close(GetTitle());
				Bootstrap.WriteStyleSheetReference(writer);
				new Link(writer).WithRel("stylesheet").WithHref("/css/site.css").Close();
			}
			using (new Body(writer).WithCustomAttribute("data-bs-theme", "dark").End())
			{
				using (new Header(writer).End())
				{
					using (new Nav(writer).WithClass("navbar navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3").End())
					{
						using (new Div(writer).WithClass("container").End())
						{
							new A(writer).WithClass("navbar-brand").WithHref("/").Close(GameFileLoader.Premium ? Localization.AssetRipperPremium : Localization.AssetRipperFree);
							using (new Button(writer)
								.WithClass("navbar-toggler")
								.WithType("button")
								.WithCustomAttribute("data-bs-toggle", "collapse")
								.WithCustomAttribute("data-bs-target", ".navbar-collapse")
								.WithCustomAttribute("aria-controls", "navbarSupportedContent")
								.WithCustomAttribute("aria-expanded", "false")
								.WithCustomAttribute("aria-label", "Toggle navigation").End())
							{
								new Span(writer).WithClass("navbar-toggler-icon").Close();
							}
							using (new Div(writer).WithClass("navbar-collapse collapse d-sm-inline-flex justify-content-between").End())
							{
								using (new Ul(writer).WithClass("navbar-nav flex-grow-1").End())
								{
									using (new Li(writer).WithClass("nav-item").End())
									{
										new A(writer).WithClass("nav-link").WithHref("/").Close(Localization.Home);
									}
									using (new Li(writer).WithClass("nav-item").End())
									{
										new A(writer).WithClass("nav-link").WithHref("/Settings/Edit").Close("Settings");
									}
									using (new Li(writer).WithClass("nav-item").End())
									{
										new A(writer).WithClass("nav-link").WithHref("/Commands").Close("Commands");
									}
								}
							}
						}
					}
				}
				using (new Div(writer).WithClass("container").End())
				{
					using (new Main(writer).WithRole("main").WithClass("pb-3").End())
					{
						WriteInnerContent(writer);
					}
				}
				using (new Footer(writer).WithClass("border-top footer text-muted").End())
				{
					using (new Div(writer).WithClass("container text-center").End())
					{
						writer.Write("&copy; 2023 - AssetRipper - ");
						new A(writer).WithHref("/Privacy").Close(Localization.Privacy);
						writer.Write(" - ");
						new A(writer).WithHref("/Licenses").Close(Localization.Licenses);
					}
				}
				writer.Write("""<script src="https://cdn.jsdelivr.net/npm/@popperjs/core@2.11.8/dist/umd/popper.min.js" integrity="sha384-I7E8VVD/ismYTF4hNIPjVp/Zjvgyol6VFvRkX/vR+Vc4jQkC+hVqc2pM8ODewa9r" crossorigin="anonymous"></script>""");
				Bootstrap.WriteScriptReference(writer);
				new Script(writer).WithSrc("/js/site.js").Close();
			}
		}
	}

	public abstract string? GetTitle();

	public abstract void WriteInnerContent(TextWriter writer);
}
