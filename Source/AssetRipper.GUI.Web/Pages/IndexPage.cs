using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages;

public sealed class IndexPage : DefaultPage
{
	public static IndexPage Instance { get; } = new();

	public override string? GetTitle() => "AssetRipper";

	public override void WriteInnerContent(TextWriter writer)
	{
		using (new Div(writer).WithClass("text-center").End())
		{
			new H1(writer).WithClass("display-4").Close(Localization.Welcome);
			if (GameFileLoader.IsLoaded)
			{
				PathLinking.WriteLink(writer, GameFileLoader.GameBundle, Localization.ViewLoadedFiles, "btn btn-success");
			}
			else
			{
				new Button(writer).WithType("button").WithClass("btn btn-secondary").WithDisabled().Close(Localization.NoFilesLoaded);
			}
			if (GameFileLoader.Premium)
			{
				//To do: Patreon Authentication
			}
			else
			{
				new Br(writer).Close();
				new Br(writer).Close();
				new P(writer).Close(Localization.DonationMessage);
				using (new Div(writer).End())
				{
					new A(writer).WithClass("btn btn-primary m-1").WithNewTabAttributes().WithHref("https://patreon.com/ds5678").Close("Patreon");
					new A(writer).WithClass("btn btn-primary m-1").WithNewTabAttributes().WithHref("https://paypal.me/ds5678").Close("Paypal");
					new A(writer).WithClass("btn btn-primary m-1").WithNewTabAttributes().WithHref("https://github.com/sponsors/ds5678").Close("GitHub Sponsors");
				}
			}
		}
	}
}
